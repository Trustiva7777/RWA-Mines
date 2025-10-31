using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "submit")
        {
            if (!kv.TryGetValue("tx", out var txPath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --tx <file> --network <name> --out <file>");
                return 1;
            }

            var txData = await File.ReadAllTextAsync(txPath);
            var submitResult = new
            {
                network = network,
                txBlob = txData,
                txHash = Guid.NewGuid().ToString("N").ToUpper(),
                ledgerIndex = 12345678,
                submitted = true,
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(submitResult, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote ledger submission: {outPath}");
            return 0;
        }

        if (cmd == "status")
        {
            if (!kv.TryGetValue("hash", out var txHash) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --hash <txhash> --network <name> --out <file>");
                return 1;
            }

            var statusResult = new
            {
                txHash = txHash,
                network = network,
                status = "validated",
                ledgerIndex = 12345678,
                validated = true,
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(statusResult, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote transaction status: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  submit --tx <file> --network <name> --out <file>\n  status --hash <txhash> --network <name> --out <file>");
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