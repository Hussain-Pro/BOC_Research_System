using System;
using System.Security.Cryptography;
using System.Text;
using BOC.Application.Common.Interfaces;

namespace BOC.Infrastructure.Security;

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var bytes = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("=", "").Replace("+", "").Replace("/", "");
    }

    public string GetQrCodeUrl(string email, string secret)
    {
        var issuer = Uri.EscapeDataString("BOC_Research");
        var account = Uri.EscapeDataString(email);
        return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code)) return false;

        try
        {
            var cleanSecret = secret.Trim();
            byte[] key;
            try
            {
                var padCount = cleanSecret.Length % 4;
                if (padCount > 0) cleanSecret += new string('=', 4 - padCount);
                key = Convert.FromBase64String(cleanSecret);
            }
            catch
            {
                key = Encoding.UTF8.GetBytes(cleanSecret);
            }

            // Get standard Unix timestamp counter
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var counter = (long)(DateTime.UtcNow - epoch).TotalSeconds / 30;
            
            for (var i = -1; i <= 1; i++)
            {
                if (GenerateTotpCode(key, counter + i) == code.Trim())
                {
                    return true;
                }
            }
        }
        catch
        {
            // Catch formatting/overflow errors
        }
        return false;
    }

    private string GenerateTotpCode(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

        var otp = binary % 1000000;
        return otp.ToString("D6");
    }
}
