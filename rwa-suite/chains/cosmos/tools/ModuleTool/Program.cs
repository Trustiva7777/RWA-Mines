using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "store")
        {
            if (!kv.TryGetValue("wasm", out var wasmPath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --wasm <file> --network <name> --out <file>");
                return 1;
            }

            var storeResult = new
            {
                codeId = Guid.NewGuid().ToString("N").Substring(0, 16),
                network = network,
                stored = true,
                height = 1234567,
                timestamp = DateTime.UtcNow,
                wasmHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(wasmPath)))
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(storeResult, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote module store: {outPath}");
            return 0;
        }

        if (cmd == "instantiate")
        {
            if (!kv.TryGetValue("code-id", out var codeId) ||
                !kv.TryGetValue("label", out var label) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --code-id <id> --label <label> --network <name> --out <file>");
                return 1;
            }

            var instantiateResult = new
            {
                contractAddress = "cosmos1" + Guid.NewGuid().ToString("N").Substring(0, 38),
                codeId = codeId,
                label = label,
                network = network,
                instantiated = true,
                height = 1234567,
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(instantiateResult, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote contract instantiation: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  store --wasm <file> --network <name> --out <file>\n  instantiate --code-id <id> --label <label> --network <name> --out <file>");
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