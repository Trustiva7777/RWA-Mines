using System.Security.Cryptography;
using System.Text.Json;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || HasFlag(args, "--help"))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            var cmd = args[0].ToLowerInvariant();
            var kv = Parse(args[1..]);
            return cmd switch
            {
                "plan-lock" => PlanLock(kv),
                "create" => CreatePolicy(kv),
                "hash" => HashPolicy(kv),
                _ => Error("Unknown command. Use --help.")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static int PlanLock(Dictionary<string,string> kv)
    {
        int days = GetInt(kv, "days", required: true);
        long currentSlot = GetLong(kv, "current-slot", required: true);
        long seconds = days * 86400L;
        long beforeSlot = currentSlot + seconds; // simple approx 1s/slot
        var outPath = kv.GetValueOrDefault("out") ?? "cardano-rwa/reports/lock_plan.json";
        var payload = new Dictionary<string, object>
        {
            ["days"] = days,
            ["currentSlot"] = currentSlot,
            ["beforeSlot"] = beforeSlot,
            ["generatedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };
        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions{WriteIndented = true}));
        var beforePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outPath))!, "lock_plan.beforeSlot");
        File.WriteAllText(beforePath, beforeSlot.ToString());
        Console.WriteLine($"Wrote: {outPath}\nWrote: {beforePath}");
        return 0;
    }

    static int CreatePolicy(Dictionary<string,string> kv)
    {
        var beforeSlot = kv.GetValueOrDefault("before-slot");
        var vkh = kv.GetValueOrDefault("vkey-hash"); // optional for key-lock
        var network = Network(kv);
        var obj = new Dictionary<string, object?>
        {
            ["type"] = "native",
            ["network"] = network,
            ["timeLock"] = string.IsNullOrWhiteSpace(beforeSlot) ? null : new { beforeSlot },
            ["keyLock"] = string.IsNullOrWhiteSpace(vkh) ? null : new { vkeyHash = vkh }
        };
        var outPath = kv.GetValueOrDefault("out") ?? "cardano-rwa/docs/policy.json";
        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(obj, new JsonSerializerOptions{WriteIndented = true}));
        Console.WriteLine($"Wrote policy: {outPath}");
        return 0;
    }

    static int HashPolicy(Dictionary<string,string> kv)
    {
        var file = kv.GetValueOrDefault("file") ?? "cardano-rwa/docs/policy.json";
        if (!File.Exists(file)) return Error($"Policy file not found: {file}");
        var sha = Sha256File(file);
        var outPath = kv.GetValueOrDefault("out") ?? "cardano-rwa/docs/policy.sha256.json";
        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(new { file, sha256 = sha }, new JsonSerializerOptions{WriteIndented = true}));
        Console.WriteLine($"Wrote: {outPath}");
        return 0;
    }

    static string Sha256File(string file)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(file);
        var hash = sha.ComputeHash(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    static bool HasFlag(string[] args, string flag) => args.Any(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));

    static string Network(Dictionary<string,string> kv)
    {
        if (kv.ContainsKey("mainnet")) return "Mainnet";
        var magic = kv.GetValueOrDefault("testnet-magic") ?? "1"; // preprod default
        return magic == "1" ? "Preprod" : (magic == "2" ? "Preview" : $"Testnet({magic})");
    }

    static int GetInt(Dictionary<string,string> kv, string key, bool required = false)
    {
        if (!kv.TryGetValue(key, out var s))
        {
            if (required) throw new ArgumentException($"Missing --{key}");
            return 0;
        }
        if (!int.TryParse(s, out var v)) throw new ArgumentException($"Invalid integer for --{key}");
        return v;
    }
    static long GetLong(Dictionary<string,string> kv, string key, bool required = false)
    {
        if (!kv.TryGetValue(key, out var s))
        {
            if (required) throw new ArgumentException($"Missing --{key}");
            return 0L;
        }
        if (!long.TryParse(s, out var v)) throw new ArgumentException($"Invalid number for --{key}");
        return v;
    }

    static void EnsureDir(string path) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!); }
    static int Error(string msg) { Console.Error.WriteLine(msg); return 1; }

    static Dictionary<string,string> Parse(string[] args)
    {
        var kv = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                var val = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
                kv[key] = val;
            }
        }
        return kv;
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"PolicyTool — Policy creation and lock planning

Usage:
  plan-lock --days <N> --current-slot <S> --out cardano-rwa/reports/lock_plan.json
  create [--before-slot <N>] [--vkey-hash <hex>] [--mainnet|--testnet-magic <N>] --out cardano-rwa/docs/policy.json
  hash --file cardano-rwa/docs/policy.json --out cardano-rwa/docs/policy.sha256.json

Examples:
  dotnet run --project tools/PolicyTool -- plan-lock --days 45 --current-slot 105000000 --out cardano-rwa/reports/lock_plan.json
  dotnet run --project tools/PolicyTool -- create --before-slot $(cat cardano-rwa/reports/lock_plan.beforeSlot) --testnet-magic 1 --out cardano-rwa/docs/policy.json
  dotnet run --project tools/PolicyTool -- hash --file cardano-rwa/docs/policy.json --out cardano-rwa/docs/policy.sha256.json

Exit codes: 0 success, 1 error");
    }
}
