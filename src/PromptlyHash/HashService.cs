using System.Security.Cryptography;

namespace PromptlyHash;

public static class HashService
{
    private static readonly string[] SupportedAlgorithms = ["MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512"];

    public static IReadOnlyList<string> Algorithms { get; } = Array.AsReadOnly(SupportedAlgorithms);

    public static async Task<string> ComputeFileHashAsync(
        string filePath,
        string algorithm,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ValidateAlgorithm(algorithm);

        cancellationToken.ThrowIfCancellationRequested();
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var hashAlgorithm = CreateAlgorithm(algorithm);
        var hash = await hashAlgorithm.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    public static bool IsValidHash(string value, string algorithm)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalized = NormalizeHash(value);
        return normalized.Length == ExpectedHexLength(algorithm)
            && normalized.All(Uri.IsHexDigit);
    }

    public static bool HashesMatch(string expected, string actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        var expectedHex = NormalizeHash(expected);
        var actualHex = NormalizeHash(actual);
        if (expectedHex.Length == 0 || expectedHex.Length != actualHex.Length)
            return false;

        try
        {
            var expectedBytes = Convert.FromHexString(expectedHex);
            var actualBytes = Convert.FromHexString(actualHex);
            return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string NormalizeHash(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new string(value.Where(c => !char.IsWhiteSpace(c) && c is not '-' and not ':').ToArray());
    }

    public static int ExpectedHexLength(string algorithm) => algorithm switch
    {
        "MD5" => 32,
        "SHA-1" => 40,
        "SHA-256" => 64,
        "SHA-384" => 96,
        "SHA-512" => 128,
        _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}", nameof(algorithm))
    };

    public static bool IsLegacyAlgorithm(string algorithm) => algorithm is "MD5" or "SHA-1";

    private static HashAlgorithm CreateAlgorithm(string algorithm)
    {
        // MD5 and SHA-1 remain available only to verify legacy checksums. The UI and
        // documentation warn against using either algorithm for new trust decisions.
#pragma warning disable CA5350, CA5351
        return algorithm switch
        {
            "MD5" => MD5.Create(),
            "SHA-1" => SHA1.Create(),
            "SHA-256" => SHA256.Create(),
            "SHA-384" => SHA384.Create(),
            "SHA-512" => SHA512.Create(),
            _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}", nameof(algorithm))
        };
#pragma warning restore CA5350, CA5351
    }

    private static void ValidateAlgorithm(string algorithm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithm);
        _ = ExpectedHexLength(algorithm);
    }
}
