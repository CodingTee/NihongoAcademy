using System.Security.Cryptography;
using System.Text;

namespace NihongoAcademy.Services;

/// <summary>
/// One-way hashing for security-question answers. The answer is normalised
/// (trimmed + lower-cased) before hashing so "John " and "john" compare equal.
/// Answers are stored only as a hash (claim "SecurityAnswerHash") so they are
/// never readable from the database.
/// </summary>
public static class SecurityAnswerHasher
{
    public static string Hash(string? answer)
    {
        var normalized = (answer ?? string.Empty).Trim().ToLowerInvariant();
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    public static bool Verify(string? answer, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }
        return Hash(answer) == storedHash;
    }
}
