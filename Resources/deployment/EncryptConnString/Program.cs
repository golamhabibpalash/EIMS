using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

// Small helper that mirrors the exact AES used by AesEncryptionHelper /
// GHPEncryptDecript inside SMS_App/Program.cs when DatabaseProvider = "SqlServer".
//
//   Usage:
//     dotnet run -- enc "Data Source=SQL\x2022;Initial Catalog=eims_db;User Id=appuser;Password=xxx"
//         -> prints the AES-128-CBC (PKCS7) base64 value to paste into appsettings.json
//     dotnet run -- dec "Ao7xIybiXyBFFa..."
//         -> prints the decrypted plaintext (for verification only)
//     dotnet run -- test "Server=...;Database=...;User ID=...;Password=..."
//         -> tests connectivity: executes SELECT 1 and lists a few tables

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  dotnet run -- enc \"<mssql-connection-string>\"");
    Console.Error.WriteLine("  dotnet run -- dec \"<encrypted-base64>\"");
    return 1;
}

string mode = args[0].ToLowerInvariant();
string value = args[1];

if (mode == "test")
{
    return await TestSqlAsync(value);
}

string aesKey = Environment.GetEnvironmentVariable("AES_KEY") ?? "1234567890123456";
string aesIv = Environment.GetEnvironmentVariable("AES_IV") ?? "1234567890123456";
byte[] key = Encoding.UTF8.GetBytes(aesKey);
byte[] iv = Encoding.UTF8.GetBytes(aesIv);

using var aes = Aes.Create();
aes.Key = key;
aes.IV = iv;
aes.Mode = CipherMode.CBC;
aes.Padding = PaddingMode.PKCS7;

switch (mode)
{
    case "enc":
        var plainBytes = Encoding.UTF8.GetBytes(value);
        var cipherBytes = aes.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        Console.WriteLine(Convert.ToBase64String(cipherBytes));
        return 0;

    case "dec":
        var inBytes = Convert.FromBase64String(value);
        var outBytes = aes.CreateDecryptor().TransformFinalBlock(inBytes, 0, inBytes.Length);
        Console.WriteLine(Encoding.UTF8.GetString(outBytes));
        return 0;

    default:
        Console.Error.WriteLine($"Unknown mode '{mode}'. Use 'enc' or 'dec'.");
        return 1;
}

static async Task<int> TestSqlAsync(string connectionString)
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        Console.WriteLine("OK - connected.");

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sys.tables";
        var tables = (int)await cmd.ExecuteScalarAsync();
        Console.WriteLine($"Existing tables in target database: {tables}");

        using var ctx = conn.CreateCommand();
        ctx.CommandText = "SELECT DB_NAME()";
        Console.WriteLine($"Database: {await ctx.ExecuteScalarAsync()}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAILED: {ex.Message}");
        return 1;
    }
}