using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SMS_App.Utilities.Notifications
{
    public enum PushSendStatus
    {
        Sent,
        NotConfigured,
        InvalidToken,
        Failed
    }

    public sealed record PushSendResult(PushSendStatus Status, int HttpStatusCode = 0, string Error = null);

    public interface IFirebasePushSender
    {
        bool IsConfigured { get; }
        Task<PushSendResult> SendAsync(
            string deviceToken,
            string title,
            string body,
            string type,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default);
    }

    public class FirebasePushSender : IFirebasePushSender
    {
        private const string MessagingScope = "https://www.googleapis.com/auth/firebase.messaging";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string ChannelId = "schoolbd_notices";

        private readonly FirebasePushOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<FirebasePushSender> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private readonly object _credentialsLock = new();

        private ServiceAccountCredentials _credentials;
        private bool _credentialsLoaded;
        private string _accessToken;
        private DateTimeOffset _accessTokenExpiresAt;

        public FirebasePushSender(
            IOptions<FirebasePushOptions> options,
            IHttpClientFactory httpClientFactory,
            IHostEnvironment environment,
            ILogger<FirebasePushSender> logger)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
            _logger = logger;
        }

        public bool IsConfigured => _options.Enabled;

        public async Task<PushSendResult> SendAsync(
            string deviceToken,
            string title,
            string body,
            string type,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return new PushSendResult(PushSendStatus.NotConfigured);
            }

            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                return new PushSendResult(PushSendStatus.InvalidToken);
            }

            var credentials = GetCredentials();
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.ProjectId))
            {
                return new PushSendResult(PushSendStatus.NotConfigured);
            }

            var accessToken = await GetAccessTokenAsync(credentials, cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new PushSendResult(PushSendStatus.Failed, 0, "Unable to obtain a Firebase access token.");
            }

            var payloadData = new Dictionary<string, string>();
            if (data != null)
            {
                foreach (var pair in data)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value != null)
                    {
                        payloadData[pair.Key] = pair.Value;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                payloadData["type"] = type;
            }

            var message = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new { title, body },
                    data = payloadData,
                    android = new
                    {
                        priority = "high",
                        notification = new
                        {
                            channel_id = ChannelId,
                            sound = "default"
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(message);
            var url = $"https://fcm.googleapis.com/v1/projects/{Uri.EscapeDataString(credentials.ProjectId)}/messages:send";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

                var client = _httpClientFactory.CreateClient();
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));

                using var response = await client.SendAsync(request, timeout.Token);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new PushSendResult(PushSendStatus.Sent, (int)response.StatusCode);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new PushSendResult(PushSendStatus.InvalidToken, (int)response.StatusCode, responseBody);
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearAccessToken();
                }

                _logger.LogWarning(
                    "Firebase push failed with status {Status}: {Body}",
                    (int)response.StatusCode,
                    Truncate(responseBody));

                return new PushSendResult(PushSendStatus.Failed, (int)response.StatusCode, responseBody);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new PushSendResult(PushSendStatus.Failed, 0, "Firebase request timed out.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase push request failed.");
                return new PushSendResult(PushSendStatus.Failed, 0, ex.Message);
            }
        }

        private ServiceAccountCredentials GetCredentials()
        {
            if (_credentialsLoaded)
            {
                return _credentials;
            }

            lock (_credentialsLock)
            {
                if (_credentialsLoaded)
                {
                    return _credentials;
                }

                _credentials = LoadCredentials();
                _credentialsLoaded = true;
                return _credentials;
            }
        }

        private ServiceAccountCredentials LoadCredentials()
        {
            try
            {
                var json = _options.ServiceAccountJson;

                if (!string.IsNullOrWhiteSpace(_options.ServiceAccountJsonBase64))
                {
                    json = Encoding.UTF8.GetString(Convert.FromBase64String(_options.ServiceAccountJsonBase64));
                }

                if (string.IsNullOrWhiteSpace(json) && !string.IsNullOrWhiteSpace(_options.ServiceAccountPath))
                {
                    var path = ResolvePath(_options.ServiceAccountPath);
                    if (File.Exists(path))
                    {
                        json = File.ReadAllText(path);
                    }
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                    if (!string.IsNullOrWhiteSpace(envPath))
                    {
                        var path = ResolvePath(envPath);
                        if (File.Exists(path))
                        {
                            json = File.ReadAllText(path);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Firebase push is enabled but no service account credentials were configured.");
                    return null;
                }

                var account = JsonSerializer.Deserialize<ServiceAccountFile>(json);
                if (account == null || string.IsNullOrWhiteSpace(account.ClientEmail)
                    || string.IsNullOrWhiteSpace(account.PrivateKey))
                {
                    _logger.LogWarning("Firebase service account JSON is missing client_email or private_key.");
                    return null;
                }

                var projectId = string.IsNullOrWhiteSpace(_options.ProjectId)
                    ? account.ProjectId
                    : _options.ProjectId;

                if (string.IsNullOrWhiteSpace(projectId))
                {
                    _logger.LogWarning("Firebase project id is not configured.");
                    return null;
                }

                return new ServiceAccountCredentials
                {
                    ProjectId = projectId,
                    ClientEmail = account.ClientEmail,
                    PrivateKey = account.PrivateKey.Replace("\\n", "\n"),
                    PrivateKeyId = account.PrivateKeyId
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load Firebase service account credentials.");
                return null;
            }
        }

        private async Task<string> GetAccessTokenAsync(ServiceAccountCredentials credentials, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    return _accessToken;
                }

                var assertion = CreateAssertion(credentials);
                using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                        ["assertion"] = assertion
                    })
                };

                var client = _httpClientFactory.CreateClient();
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));

                using var response = await client.SendAsync(request, timeout.Token);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Firebase OAuth token request failed with status {Status}: {Body}",
                        (int)response.StatusCode,
                        Truncate(responseBody));
                    return null;
                }

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
                if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                {
                    _logger.LogWarning("Firebase OAuth token response did not contain an access_token.");
                    return null;
                }

                var expiresIn = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600;
                _accessToken = tokenResponse.AccessToken;
                _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
                return _accessToken;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Firebase OAuth token request timed out.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase OAuth token request failed.");
                return null;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private void ClearAccessToken()
        {
            _accessToken = null;
            _accessTokenExpiresAt = DateTimeOffset.MinValue;
        }

        private static string CreateAssertion(ServiceAccountCredentials credentials)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(credentials.PrivateKey);

            var now = DateTimeOffset.UtcNow;
            var header = new JwtHeader(new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256));

            if (!string.IsNullOrWhiteSpace(credentials.PrivateKeyId))
            {
                header["kid"] = credentials.PrivateKeyId;
            }

            var payload = new JwtPayload
            {
                { "iss", credentials.ClientEmail },
                { "scope", MessagingScope },
                { "aud", TokenEndpoint },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(55).ToUnixTimeSeconds() }
            };

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(_environment.ContentRootPath, path);
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= 500 ? value : value.Substring(0, 500);
        }

        private sealed class ServiceAccountCredentials
        {
            public string ProjectId { get; set; }
            public string ClientEmail { get; set; }
            public string PrivateKey { get; set; }
            public string PrivateKeyId { get; set; }
        }

        private sealed class ServiceAccountFile
        {
            [JsonPropertyName("project_id")]
            public string ProjectId { get; set; }

            [JsonPropertyName("client_email")]
            public string ClientEmail { get; set; }

            [JsonPropertyName("private_key")]
            public string PrivateKey { get; set; }

            [JsonPropertyName("private_key_id")]
            public string PrivateKeyId { get; set; }
        }

        private sealed class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
