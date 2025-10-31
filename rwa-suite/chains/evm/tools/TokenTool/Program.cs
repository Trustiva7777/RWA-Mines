using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "mint")
        {
            if (!kv.TryGetValue("contract", out var contractAddr) ||
                !kv.TryGetValue("to", out var toAddr) ||
                !kv.TryGetValue("amount", out var amountStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --contract <addr> --to <addr> --amount <n> --network <name> --out <file>");
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
                contract = contractAddr,
                to = toAddr,
                amount = amount,
                network = network,
                txHash = "0x" + Guid.NewGuid().ToString("N"),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(mintTx, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote mint transaction: {outPath}");
            return 0;
        }

        if (cmd == "transfer")
        {
            if (!kv.TryGetValue("contract", out var contractAddr) ||
                !kv.TryGetValue("from", out var fromAddr) ||
                !kv.TryGetValue("to", out var toAddr) ||
                !kv.TryGetValue("amount", out var amountStr) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --contract <addr> --from <addr> --to <addr> --amount <n> --network <name> --out <file>");
                return 1;
            }

            if (!ulong.TryParse(amountStr, out var amount))
            {
                Console.Error.WriteLine("Invalid amount");
                return 1;
            }

            var transferTx = new
            {
                type = "transfer",
                contract = contractAddr,
                from = fromAddr,
                to = toAddr,
                amount = amount,
                network = network,
                txHash = "0x" + Guid.NewGuid().ToString("N"),
                timestamp = DateTime.UtcNow
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(transferTx, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote transfer transaction: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  mint --contract <addr> --to <addr> --amount <n> --network <name> --out <file>\n  transfer --contract <addr> --from <addr> --to <addr> --amount <n> --network <name> --out <file>");
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