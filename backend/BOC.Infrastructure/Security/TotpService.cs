using System;
using System.Security.Cryptography;
using System.Text;
using BOC.Application.Common.Interfaces;

namespace BOC.Infrastructure.Security;

public class TotpService : ITotpService
{
    // Base32 alphabet as used by Google Authenticator (RFC 4648)
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Generates a random 20-byte secret encoded as Base32 (compatible with TOTP apps).
    /// </summary>
    public string GenerateSecret()
    {
        var bytes = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base32Encode(bytes);
    }

    /// <summary>
    /// Returns the otpauth:// URI with a Base32-encoded secret for QR scanning.
    /// </summary>
    public string GetQrCodeUrl(string email, string secret)
    {
        var issuer  = Uri.EscapeDataString("BOC_Research");
        var account = Uri.EscapeDataString(email);
        // secret is already Base32 — pass it as-is
        return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }

    /// <summary>
    /// Verifies a 6-digit TOTP code against the stored Base32 secret.
    /// Accepts codes from the previous, current, and next 30-second window.
    /// </summary>
    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code)) return false;

        try
        {
            var key = Base32Decode(secret.Trim().ToUpperInvariant());

            var epoch   = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var counter = (long)(DateTime.UtcNow - epoch).TotalSeconds / 30;

            // Allow ±1 window for clock drift
            for (var i = -1; i <= 1; i++)
            {
                if (GenerateTotpCode(key, counter + i) == code.Trim())
                    return true;
            }
        }
        catch
        {
            // Invalid secret format
        }
        return false;
    }

    // ─── TOTP Core ────────────────────────────────────────────────────────────

    private static string GenerateTotpCode(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash   = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;

        var binary = ((hash[offset]     & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) <<  8)
                   |  (hash[offset + 3] & 0xFF);

        return (binary % 1_000_000).ToString("D6");
    }

    // ─── Base32 Helpers ───────────────────────────────────────────────────────

    private static string Base32Encode(byte[] bytes)
    {
        var sb       = new StringBuilder(((bytes.Length * 8) + 4) / 5);
        int buffer   = bytes[0];
        int bitsLeft = 8;
        int pos      = 1;

        while (bitsLeft > 0 || pos < bytes.Length)
        {
            if (bitsLeft < 5)
            {
                if (pos < bytes.Length)
                {
                    buffer   = (buffer << 8) | (bytes[pos++] & 0xFF);
                    bitsLeft += 8;
                }
                else
                {
                    buffer   <<= (5 - bitsLeft);
                    bitsLeft   = 5;
                }
            }
            bitsLeft -= 5;
            sb.Append(Base32Chars[(buffer >> bitsLeft) & 0x1F]);
        }
        return sb.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        // Strip padding
        input = input.TrimEnd('=');

        var byteCount = input.Length * 5 / 8;
        var result    = new byte[byteCount];
        int buffer    = 0;
        int bitsLeft  = 0;
        int index     = 0;

        foreach (var ch in input)
        {
            var val = Base32Chars.IndexOf(ch);
            if (val < 0)
                throw new FormatException($"Invalid Base32 character: '{ch}'");

            buffer   = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                result[index++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }
        return result;
    }
}
