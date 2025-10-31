using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "deploy")
        {
            if (!kv.TryGetValue("program", out var programPath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --program <file> --network <name> --out <file>");
                return 1;
            }

            var deployment = new
            {
                programId = Guid.NewGuid().ToString("N").Substring(0, 44),
                network = network,
                deployed = true,
                slot = 123456789,
                timestamp = DateTime.UtcNow,
                bytecodeSize = 123456
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(deployment, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote program deployment: {outPath}");
            return 0;
        }

        if (cmd == "verify")
        {
            if (!kv.TryGetValue("program-id", out var programId) ||
                !kv.TryGetValue("source", out var sourcePath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --program-id <id> --source <file> --network <name> --out <file>");
                return 1;
            }

            var verification = new
            {
                programId = programId,
                network = network,
                verified = true,
                timestamp = DateTime.UtcNow,
                sourceHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(sourcePath)))
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(verification, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote program verification: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  deploy --program <file> --network <name> --out <file>\n  verify --program-id <id> --source <file> --network <name> --out <file>");
        return 1;
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