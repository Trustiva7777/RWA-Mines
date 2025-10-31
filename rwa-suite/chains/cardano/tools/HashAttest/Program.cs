using System.Security.Cryptography;
using System.Text.Json;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (string.Equals(cmd, "manifest", StringComparison.OrdinalIgnoreCase))
        {
            var dir = kv.GetValueOrDefault("dir") ?? "chains/cardano/docs";
            var outPath = kv.GetValueOrDefault("out") ?? Path.Combine(dir, "sha256-manifest.json");
            var absDir = Path.GetFullPath(dir);
            if (!Directory.Exists(absDir)) Directory.CreateDirectory(absDir);

            var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var file in HashUtil.WalkFiles(absDir))
            {
                var rel = Path.GetRelativePath(absDir, file).Replace('\\', '/');
                map[rel] = HashUtil.Sha256File(file);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote manifest: {outPath}");
            return 0;
        }

        if (string.Equals(cmd, "attest", StringComparison.OrdinalIgnoreCase))
        {
            string required(string key)
            {
                if (!kv.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v)) throw new ArgumentException($"Missing --{key}");
                return v;
            }
            var policy = required("policy");
            var network = required("network");
            var manifest = required("manifest");
            var allowlistFile = required("allowlist-file");
            var outPath = kv.GetValueOrDefault("out") ?? $"chains/cardano/docs/token/attestation.{network}.json";
            var att = new Dictionary<string, object?>
            {
                ["policyId"] = policy,
                ["network"] = network,
                ["manifestSha256"] = HashUtil.Sha256File(manifest),
                ["allowlistSha256"] = HashUtil.Sha256File(allowlistFile),
                ["timestamp"] = DateTimeOffset.UtcNow.ToString("O")
            };
            if (kv.TryGetValue("policy-json", out var pjson) && File.Exists(pjson))
                att["policyJsonSha256"] = HashUtil.Sha256File(pjson);
            if (kv.TryGetValue("before-slot", out var before))
                att["beforeSlot"] = before;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(att, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote attestation: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  manifest --dir <dir> --out <file>\n  attest --policy <id> --network <name> --manifest <file> --allowlist-file <file> [--policy-json <file>] [--before-slot <n>] --out <file>");
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

internal static class HashUtil
{
    public static string Sha256File(string file)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(file);
        var hash = sha.ComputeHash(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static IEnumerable<string> WalkFiles(string dir)
    {
        foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(f);
            if (name.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase)) continue;
            yield return f;
        }
    }
}
