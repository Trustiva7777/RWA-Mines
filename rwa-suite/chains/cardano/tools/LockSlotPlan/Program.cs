using System.Text.Json;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var kv = Parse(args);
        if (!kv.TryGetValue("days", out var daysStr))
        {
            Console.Error.WriteLine("Usage: plan --days <n> [--current-slot <n>] --out <file>");
            return 1;
        }
        int days = int.TryParse(daysStr, out var d) ? d : 0;
        long currentSlot = 0;
        if (kv.TryGetValue("current-slot", out var cs) && long.TryParse(cs, out var csn)) currentSlot = csn;

        // Approximate: 1 second per slot
        long seconds = days * 86400L;
        long beforeSlot = currentSlot + seconds; // simplistic approximation

        var outPath = kv.GetValueOrDefault("out") ?? "chains/cardano/reports/lock_plan.json";
        var payload = new Dictionary<string, object>
        {
            ["days"] = days,
            ["currentSlot"] = currentSlot,
            ["beforeSlot"] = beforeSlot,
            ["generatedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

        // Also write helper file with just the number
        var beforeSlotPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outPath))!, "lock_plan.beforeSlot");
        await File.WriteAllTextAsync(beforeSlotPath, beforeSlot.ToString());

        Console.WriteLine($"Wrote: {outPath} and {beforeSlotPath}");
        return 0;
    }

    static Dictionary<string, string> Parse(string[] args)
    {
        var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith("--"))
            {
                var key = a[2..];
                string val = "true";
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--")) val = args[++i];
                kv[key] = val;
            }
        }
        return kv;
    }
}
