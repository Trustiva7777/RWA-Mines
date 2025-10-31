using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "plan")
        {
            if (!kv.TryGetValue("manifest", out var manifestPath) ||
                !kv.TryGetValue("total-ada", out var totalAdaStr) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --manifest <file> --total-ada <n> --out <file>");
                return 1;
            }

            if (!long.TryParse(totalAdaStr, out var totalAda))
            {
                Console.Error.WriteLine("Invalid --total-ada value");
                return 1;
            }

            var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(manifestPath));
            if (manifest == null)
            {
                Console.Error.WriteLine("Invalid manifest JSON");
                return 1;
            }

            // For demo, assume each file gets equal share (deterministic based on sorted keys)
            var sortedKeys = manifest.Keys.OrderBy(k => k).ToList();
            var share = totalAda / sortedKeys.Count;
            var payouts = new Dictionary<string, long>();
            foreach (var key in sortedKeys)
            {
                payouts[key] = share;
            }

            var result = new
            {
                totalAda = totalAda,
                payouts = payouts
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote payout plan: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  plan --manifest <file> --total-ada <n> --out <file>");
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