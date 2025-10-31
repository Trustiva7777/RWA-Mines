using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "create")
        {
            if (!kv.TryGetValue("name", out var name) ||
                !kv.TryGetValue("symbol", out var symbol) ||
                !kv.TryGetValue("decimals", out var decimalsStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --name <name> --symbol <sym> --decimals <n> --network <name> --out <file>");
                return 1;
            }

            if (!int.TryParse(decimalsStr, out var decimals))
            {
                Console.Error.WriteLine("Invalid decimals");
                return 1;
            }

            var token = new
            {
                type = "create",
                name = name,
                symbol = symbol,
                decimals = decimals,
                network = network,
                mintAddress = Guid.NewGuid().ToString("N").Substring(0, 44),
                txHash = Guid.NewGuid().ToString("N"),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(token, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote token creation: {outPath}");
            return 0;
        }

        if (cmd == "mint")
        {
            if (!kv.TryGetValue("mint", out var mintAddr) ||
                !kv.TryGetValue("to", out var toAddr) ||
                !kv.TryGetValue("amount", out var amountStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --mint <addr> --to <addr> --amount <n> --network <name> --out <file>");
                return 1;
            }

            if (!ulong.TryParse(amountStr, out var amount))
            {
                Console.Error.WriteLine("Invalid amount");
                return 1;
            }

            var mintTx = new
            {
                type = "mint",
                mint = mintAddr,
                to = toAddr,
                amount = amount,
                network = network,
                txHash = Guid.NewGuid().ToString("N"),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(mintTx, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote token mint: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  create --name <name> --symbol <sym> --decimals <n> --network <name> --out <file>\n  mint --mint <addr> --to <addr> --amount <n> --network <name> --out <file>");
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