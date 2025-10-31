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
                "plan" => Plan(kv),
                "ada-ledger" => AdaLedger(kv),
                _ => Err("Unknown command. Use --help.")
            };
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    static int Plan(Dictionary<string,string> kv)
    {
        var csv = Req(kv, "csv");
        var outPath = kv.GetValueOrDefault("out") ?? "cardano-rwa/reports/payout_plan.json";
        var rows = ReadCsv(csv);
        var plan = rows.Select(r => new
        {
            address = r.GetValueOrDefault("wallet_address") ?? r.Values.FirstOrDefault() ?? string.Empty,
            tokens = r.GetValueOrDefault("tokens_held") ?? r.GetValueOrDefault("tokens") ?? "0"
        }).Where(x => !string.IsNullOrWhiteSpace(x.address)).ToArray();
        EnsureDir(outPath);
        File.WriteAllText(outPath, JsonSerializer.Serialize(plan, new JsonSerializerOptions{WriteIndented = true}));
        Console.WriteLine($"Wrote payout plan: {outPath}");
        return 0;
    }

    static int AdaLedger(Dictionary<string,string> kv)
    {
        var csv = Req(kv, "csv");
        var fxStr = kv.GetValueOrDefault("fx") ?? "0.33";
        if (!decimal.TryParse(fxStr, out var fx)) return Err("Invalid --fx");
        var outPath = kv.GetValueOrDefault("out") ?? "cardano-rwa/reports/ada_ledger.csv";
        var rows = ReadCsv(csv);
        var lines = new List<string> { "address,ada" };
        foreach (var r in rows)
        {
            var addr = r.GetValueOrDefault("wallet_address") ?? r.Values.FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(addr)) continue;
            var usdStr = r.GetValueOrDefault("usd") ?? r.GetValueOrDefault("usd_amount") ?? r.GetValueOrDefault("tokens_held") ?? "0";
            if (!decimal.TryParse(usdStr, out var usd)) usd = 0m;
            var ada = usd * fx;
            lines.Add($"{addr},{ada:0.########}");
        }
        EnsureDir(outPath);
        File.WriteAllLines(outPath, lines);
        Console.WriteLine($"Wrote ADA ledger: {outPath}");
        return 0;
    }

    static void EnsureDir(string p) => Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(p))!);
    static int Err(string m){ Console.Error.WriteLine(m); return 1; }
    static string Req(Dictionary<string,string> kv, string k){ return kv.TryGetValue(k, out var v) ? v : throw new ArgumentException($"Missing --{k}"); }
    static Dictionary<string,string> Parse(string[] a){ var kv=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<a.Length;i++){ if(a[i].StartsWith("--")){ var k=a[i][2..]; var v=(i+1<a.Length && !a[i+1].StartsWith("--"))?a[++i]:"true"; kv[k]=v; }} return kv; }
    static List<Dictionary<string,string>> ReadCsv(string path){ var lines=File.ReadAllLines(path).Where(l=>!string.IsNullOrWhiteSpace(l)).ToArray(); if(lines.Length==0) return new(); var hdr=lines[0].Split(',').Select(s=>s.Trim(' ','"')).ToArray(); var list=new List<Dictionary<string,string>>(); foreach(var line in lines.Skip(1)){ var cols=line.Split(','); var row=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<hdr.Length && i<cols.Length;i++){ row[hdr[i]]=cols[i].Trim(' ','"'); } list.Add(row);} return list; }

    static void PrintHelp(){ Console.WriteLine(@"PayoutPlan — Plan and ADA ledger

Usage:
  plan --csv cardano-rwa/docs/allocations.csv --out cardano-rwa/reports/payout_plan.json
  ada-ledger --csv cardano-rwa/docs/allocations.csv --fx 0.33 --out cardano-rwa/reports/ada_ledger.csv

Exit codes: 0 success, 1 error"); }
}
