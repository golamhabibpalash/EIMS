using System;
using QRCoder;

namespace SMS_App.Utilities;

/// <summary>
/// Generates QR codes as raw PNG bytes using QRCoder's <see cref="PngByteQRCode"/>, which is
/// pure-managed (no System.Drawing / native dependency) so it renders identically on the
/// Linux/systemd deployment targets as it does on Windows.
/// </summary>
public static class QrCodeHelper
{
    /// <summary>Encodes <paramref name="payload"/> as a QR PNG. Returns null for empty input.</summary>
    public static byte[] GeneratePng(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload)) return null;

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }

    /// <summary>Encodes <paramref name="payload"/> as a base64 PNG data URI for use in an &lt;img&gt; src.</summary>
    public static string GenerateDataUri(string payload, int pixelsPerModule = 10)
    {
        var png = GeneratePng(payload, pixelsPerModule);
        return png == null ? string.Empty : "data:image/png;base64," + Convert.ToBase64String(png);
    }
}
