using System.Security.Cryptography;
using System.Text;

namespace Share.Utils;

public static class ToolHash
{
    public static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public static bool IsHexString(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return false;

        foreach (char c in s)
            if (!Uri.IsHexDigit(c))
                return false;

        return true;
    }


    public static string Bcrypt(string value)
    {
        return BCrypt.Net.BCrypt.HashPassword(value);
    }

    public static bool Verify(string raw, string hashed)
    {
        if (hashed.StartsWith("$2"))
            return BCrypt.Net.BCrypt.Verify(raw, hashed);

        return Sha256(raw).Equals(hashed, StringComparison.OrdinalIgnoreCase);
    }

    // Share/Utils/ToolHash.cs
public static bool IsHashed(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return false;

    // 1. bcrypt 패턴 확인
    if (value.StartsWith("$2")) return true;

    // 2. SHA256 패턴 확인 (IsHashed 재호출 대신 IsHexString 사용)
    if (value.Length == 64 && IsHexString(value)) 
        return true;

    return false;
}
}