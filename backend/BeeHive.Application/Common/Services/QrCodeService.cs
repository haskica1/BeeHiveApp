using QRCoder;

namespace BeeHive.Application.Common.Services;

public interface IQrCodeService
{
    /// <summary>
    /// Generates a QR code for <paramref name="content"/> and returns it as a Base64-encoded PNG string.
    /// </summary>
    string GeneratePngBase64(string content);
}

public class QrCodeService : IQrCodeService
{
    public string GeneratePngBase64(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data      = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var code      = new PngByteQRCode(data);

        // 10 px per module → roughly 250×250 px for a UUID payload
        var png = code.GetGraphic(pixelsPerModule: 10);
        return Convert.ToBase64String(png);
    }
}
