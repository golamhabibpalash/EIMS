using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SMS.BLL.MultiTenancy
{
    /// <summary>Database login for one tenant database. Lives only in memory; stored encrypted.</summary>
    public sealed record DatabaseCredentials(string UserName, string Password)
    {
        public override string ToString() => $"{UserName} (password hidden)";
    }

    public interface ISecretProtector
    {
        int KeyVersion { get; }
        string Protect(string plaintext);
        string Unprotect(string protectedValue);
    }

    /// <summary>
    /// AES-256-GCM with a master key that is NOT stored in any database (env var
    /// SCHOOLBD_SECRETS_KEY / config MultiTenancy:SecretsKey, base64 of 32 random bytes).
    /// Format: base64( nonce[12] | tag[16] | ciphertext ).
    /// </summary>
    public sealed class AesGcmSecretProtector : ISecretProtector
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private readonly byte[] _key;

        public AesGcmSecretProtector(byte[] key, int keyVersion = 1)
        {
            if (key == null || key.Length != 32)
            {
                throw new ArgumentException("The platform secrets key must be exactly 32 bytes (base64 of 32 random bytes).");
            }
            _key = key;
            KeyVersion = keyVersion;
        }

        public static AesGcmSecretProtector FromBase64(string base64Key)
        {
            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new InvalidOperationException(
                    "Multi-tenancy needs a secrets key: set SCHOOLBD_SECRETS_KEY (or MultiTenancy:SecretsKey) to base64 of 32 random bytes, e.g. `openssl rand -base64 32`.");
            }
            byte[] key;
            try
            {
                key = Convert.FromBase64String(base64Key.Trim());
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("SCHOOLBD_SECRETS_KEY is not valid base64.");
            }
            return new AesGcmSecretProtector(key);
        }

        public int KeyVersion { get; }

        public string Protect(string plaintext)
        {
            var plain = Encoding.UTF8.GetBytes(plaintext ?? string.Empty);
            var output = new byte[NonceSize + TagSize + plain.Length];
            var nonce = output.AsSpan(0, NonceSize);
            RandomNumberGenerator.Fill(nonce);
            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plain, output.AsSpan(NonceSize + TagSize), output.AsSpan(NonceSize, TagSize));
            return Convert.ToBase64String(output);
        }

        public string Unprotect(string protectedValue)
        {
            var data = Convert.FromBase64String(protectedValue);
            if (data.Length < NonceSize + TagSize)
            {
                throw new CryptographicException("Protected value is too short.");
            }
            var plain = new byte[data.Length - NonceSize - TagSize];
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(data.AsSpan(0, NonceSize), data.AsSpan(NonceSize + TagSize), data.AsSpan(NonceSize, TagSize), plain);
            return Encoding.UTF8.GetString(plain);
        }
    }

    public static class CredentialsJson
    {
        private sealed class Dto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public static string Serialize(DatabaseCredentials credentials) =>
            JsonSerializer.Serialize(new Dto { UserName = credentials.UserName, Password = credentials.Password });

        public static DatabaseCredentials Deserialize(string json)
        {
            var dto = JsonSerializer.Deserialize<Dto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto == null || string.IsNullOrEmpty(dto.UserName))
            {
                throw new InvalidOperationException("Stored database credentials are malformed.");
            }
            return new DatabaseCredentials(dto.UserName, dto.Password ?? string.Empty);
        }
    }
}
