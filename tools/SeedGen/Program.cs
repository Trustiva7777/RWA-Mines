using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help")) { PrintHelp(); return 0; }
        try
        {
            var kv = Parse(args);
            var outDir = kv.GetValueOrDefault("out") ?? "Wallet";
            var pass = Environment.GetEnvironmentVariable("SEEDGEN_PASSPHRASE");
            if (string.IsNullOrEmpty(pass)) return Err("Set SEEDGEN_PASSPHRASE environment variable.");
            Directory.CreateDirectory(outDir);

            // Generate raw 32-byte seed and encrypt it
            var seed = RandomNumberGenerator.GetBytes(32);
            var enc = Encrypt(seed, pass);
            File.WriteAllBytes(Path.Combine(outDir, "seed.enc"), enc);
            File.WriteAllText(Path.Combine(outDir, "seed.enc.info"), "AES-256-GCM with PBKDF2-SHA512, iters=200000");

            // Derive pseudo-xpubs and addresses by hashing (placeholder; replace with CardanoSharp in production)
            WriteAccountArtifacts(Path.Combine(outDir, "mainnet"), "Mainnet", seed);
            WriteAccountArtifacts(Path.Combine(outDir, "preprod"), "Preprod", seed);
            WriteAccountArtifacts(Path.Combine(outDir, "preview"), "Preview", seed);

            Console.WriteLine($"Wrote encrypted seed and addresses under: {outDir}");
            return 0;
        }
        catch (Exception ex){ Console.Error.WriteLine(ex.Message); return 1; }
    }

    static void WriteAccountArtifacts(string dir, string network, byte[] seed)
    {
        Directory.CreateDirectory(dir);
        var xpub = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(network + ":pay:" + Convert.ToHexString(seed)))).ToLowerInvariant();
        var spub = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(network + ":stake:" + Convert.ToHexString(seed)))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(dir, "pay.xpub"), xpub);
        File.WriteAllText(Path.Combine(dir, "stake.xpub"), spub);
        File.WriteAllText(Path.Combine(dir, "addr_0.txt"), $"addr_{network.ToLower()}_" + xpub[..16]);
    }

    static byte[] Encrypt(byte[] data, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var derive = new Rfc2898DeriveBytes(password, salt, 200_000, HashAlgorithmName.SHA512);
        var key = derive.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[data.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(key, 16)) { aes.Encrypt(nonce, data, cipher, tag); }
        var output = new byte[16 + 12 + 16 + cipher.Length];
        Buffer.BlockCopy(salt, 0, output, 0, 16);
        Buffer.BlockCopy(nonce, 0, output, 16, 12);
        Buffer.BlockCopy(tag, 0, output, 28, 16);
        Buffer.BlockCopy(cipher, 0, output, 44, cipher.Length);
        return output;
    }

    static int Err(string m){ Console.Error.WriteLine(m); return 1; }
    static Dictionary<string,string> Parse(string[] a){ var kv=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<a.Length;i++){ if(a[i].StartsWith("--")){ var k=a[i][2..]; var v=(i+1<a.Length && !a[i+1].StartsWith("--"))?a[++i]:"true"; kv[k]=v; }} return kv; }
    static void PrintHelp(){ Console.WriteLine(@"SeedGen — Encrypted seed and addresses

Usage:
  SEEDGEN_PASSPHRASE=... dotnet run --project tools/SeedGen -- --out Wallet

Outputs:
  Wallet/seed.enc, Wallet/*/pay.xpub, Wallet/*/stake.xpub, Wallet/*/addr_0.txt

Exit codes: 0 success, 1 error"); }
}
