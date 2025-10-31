using System.Security.Cryptography;
using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "generate")
        {
            if (!kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing --out <file>");
                return 1;
            }

            // Generate 32-byte seed
            var seed = RandomNumberGenerator.GetBytes(32);

            // Encrypt with passphrase (default if not provided)
            var passphrase = kv.GetValueOrDefault("passphrase", "default-passphrase-change-me");
            var salt = RandomNumberGenerator.GetBytes(16);
            var key = DeriveKey(passphrase, salt);

            var encrypted = Encrypt(seed, key);

            var result = new
            {
                encryptedSeed = Convert.ToBase64String(encrypted),
                salt = Convert.ToBase64String(salt),
                algorithm = "AES-256-GCM"
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote encrypted seed: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  generate --out <file> [--passphrase <phrase>]");
        return 1;
    }

    static byte[] DeriveKey(string passphrase, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    static byte[] Encrypt(byte[] data, byte[] key)
    {
        using var aes = new AesGcm(key, 16);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[data.Length];
        aes.Encrypt(nonce, data, ciphertext, tag);
        return nonce.Concat(ciphertext).Concat(tag).ToArray();
    }
}

internal static class Argx
{
    public static (string cmd, Dictionary<string, string> kv) Parse(string[] args)
    {
        var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cmd = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : string.Empty;
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith("--"))
            {
                var key = a[2..];
                string val = "true";
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--")) { val = args[++i]; }
                kv[key] = val;
            }
        }
        return (cmd, kv);
    }
}