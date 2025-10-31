using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "issue")
        {
            if (!kv.TryGetValue("issuer", out var issuer) ||
                !kv.TryGetValue("currency", out var currency) ||
                !kv.TryGetValue("amount", out var amountStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --issuer <addr> --currency <code> --amount <n> --network <name> --out <file>");
                return 1;
            }

            if (!decimal.TryParse(amountStr, out var amount))
            {
                Console.Error.WriteLine("Invalid amount");
                return 1;
            }

            var issueTx = new
            {
                type = "issue",
                issuer = issuer,
                currency = currency,
                amount = amount,
                network = network,
                txHash = Guid.NewGuid().ToString("N").ToUpper(),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(issueTx, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote token issue transaction: {outPath}");
            return 0;
        }

        if (cmd == "transfer")
        {
            if (!kv.TryGetValue("from", out var fromAddr) ||
                !kv.TryGetValue("to", out var toAddr) ||
                !kv.TryGetValue("currency", out var currency) ||
                !kv.TryGetValue("issuer", out var issuer) ||
                !kv.TryGetValue("amount", out var amountStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --from <addr> --to <addr> --currency <code> --issuer <addr> --amount <n> --network <name> --out <file>");
                return 1;
            }

            if (!decimal.TryParse(amountStr, out var amount))
            {
                Console.Error.WriteLine("Invalid amount");
                return 1;
            }

            var transferTx = new
            {
                type = "transfer",
                from = fromAddr,
                to = toAddr,
                currency = currency,
                issuer = issuer,
                amount = amount,
                network = network,
                txHash = Guid.NewGuid().ToString("N").ToUpper(),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(transferTx, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote token transfer transaction: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  issue --issuer <addr> --currency <code> --amount <n> --network <name> --out <file>\n  transfer --from <addr> --to <addr> --currency <code> --issuer <addr> --amount <n> --network <name> --out <file>");
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