using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help")) { PrintHelp(); return 0; }
        try
        {
            var cmd = args[0].ToLowerInvariant();
            var kv = Parse(args[1..]);
            return cmd switch
            {
                "manifest" => Manifest(kv),
                "attest" => Attest(kv),
                _ => Error("Unknown command. Use --help.")
            };
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    static int Manifest(Dictionary<string,string> kv)
    {
        var dir = kv.GetValueOrDefault("dir") ?? "cardano-rwa/docs";
        var outPath = kv.GetValueOrDefault("out") ?? Path.Combine(dir, "sha256-manifest.json");
        var abs = Path.GetFullPath(dir);
        if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);
        var map = new SortedDictionary<string,string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(abs, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            if (name.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase)) continue;
            var rel = Path.GetRelativePath(abs, file).Replace('\\', '/');
            map[rel] = Sha256File(file);
        }
        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(map, new JsonSerializerOptions{WriteIndented = true}));
        Console.WriteLine($"Wrote manifest: {outPath}");
        return 0;
    }

    static int Attest(Dictionary<string,string> kv)
    {
        string required(string k) => kv.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v) ? v : throw new ArgumentException($"Missing --{k}");
        var policy = required("policy");
        var network = Network(kv);
        var series = kv.GetValueOrDefault("series") ?? "RWA Series";
        var manifestPath = required("manifest");
        var allowlist = required("allowlist-file");
        var policyJson = kv.GetValueOrDefault("policy-json");
        var beforeSlot = kv.GetValueOrDefault("before-slot");
        var outPath = kv.GetValueOrDefault("out") ?? $"cardano-rwa/docs/attestation.{network}.json";

        var att = new Dictionary<string, object?>
        {
            ["series"] = series,
            ["network"] = network,
            ["policyId"] = policy,
            ["manifestSha256"] = Sha256File(manifestPath),
            ["allowlistSha256"] = CanonicalAllowlistSha(allowlist),
            ["timestamp"] = DateTimeOffset.UtcNow.ToString("O")
        };
        if (!string.IsNullOrWhiteSpace(policyJson)) att["policyJsonSha256"] = Sha256File(policyJson!);
        if (!string.IsNullOrWhiteSpace(beforeSlot)) att["beforeSlot"] = beforeSlot;

        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(att, new JsonSerializerOptions{WriteIndented = true}));
        Console.WriteLine($"Wrote attestation: {outPath}");
        return 0;
    }

    static string CanonicalAllowlistSha(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        var list = new List<string>();
        if (ext is ".csv")
        {
            var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            var hdr = lines[0].Split(',').Select(s => s.Trim()).ToArray();
            var idx = Array.FindIndex(hdr, h => h.Equals("address", StringComparison.OrdinalIgnoreCase));
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                var v = (idx >= 0 && idx < cols.Length) ? cols[idx] : cols[0];
                list.Add(v.Trim().ToLowerInvariant());
            }
        }
        else if (ext is ".json")
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray()) list.Add(el.GetString()!.Trim().ToLowerInvariant());
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("addresses", out var arr))
            {
                foreach (var el in arr.EnumerateArray()) list.Add(el.GetString()!.Trim().ToLowerInvariant());
            }
        }
        else throw new ArgumentException("Allowlist must be CSV or JSON");
        list.Sort(StringComparer.Ordinal);
        var canonical = string.Join('\n', list);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    static string Sha256File(string file) { using var sha = SHA256.Create(); using var fs = File.OpenRead(file); return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant(); }
    static string Network(Dictionary<string,string> kv)
    {
        if (kv.ContainsKey("mainnet")) return "Mainnet";
        var magic = kv.GetValueOrDefault("testnet-magic") ?? "1";
        return magic == "1" ? "Preprod" : (magic == "2" ? "Preview" : $"Testnet({magic})");
    }
    static void EnsureDir(string path) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!); }
    static Dictionary<string,string> Parse(string[] a){ var kv=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<a.Length;i++){ if(a[i].StartsWith("--")){ var k=a[i][2..]; var v=(i+1<a.Length && !a[i+1].StartsWith("--"))?a[++i]:"true"; kv[k]=v; }} return kv; }
    static int Error(string m){ Console.Error.WriteLine(m); return 1; }
    static void PrintHelp(){ Console.WriteLine(@"HashAttest — Proofs and Attestations

Usage:
  manifest --dir cardano-rwa/docs --out cardano-rwa/docs/sha256-manifest.json
  attest --policy <id> [--series <name>] [--mainnet|--testnet-magic <N>] \
         --manifest cardano-rwa/docs/sha256-manifest.json \
         --allowlist-file cardano-rwa/docs/allowlist.csv \
         [--policy-json cardano-rwa/docs/policy.json] [--before-slot <N>] \
         --out cardano-rwa/docs/attestation.Preprod.json

Exit codes: 0 success, 1 error"); }
}
