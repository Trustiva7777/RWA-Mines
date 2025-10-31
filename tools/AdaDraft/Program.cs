using System.Text.Json;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help")) { PrintHelp(); return 0; }
        try
        {
            var kv = Parse(args);
            var ledger = Req(kv, "ledger");
            var outDir = kv.GetValueOrDefault("out") ?? "cardano-rwa/out/tx_batches";
            var policy = kv.GetValueOrDefault("policy") ?? "";
            var network = Network(kv);
            int max = int.TryParse(kv.GetValueOrDefault("max-outputs"), out var m) ? m : 80;
            var rows = ReadCsv(ledger);
            var outputs = new List<Dictionary<string, object?>>();
            foreach (var r in rows)
            {
                var addr = r.GetValueOrDefault("address") ?? r.Values.FirstOrDefault() ?? string.Empty;
                var adaStr = r.GetValueOrDefault("ada") ?? "0";
                if (string.IsNullOrWhiteSpace(addr)) continue;
                if (!decimal.TryParse(adaStr, out var ada)) ada = 0m;
                outputs.Add(new Dictionary<string, object?> { ["address"] = addr, ["ada"] = ada });
            }
            Directory.CreateDirectory(outDir);
            var batchPath = Path.Combine(outDir, "tx_batch_001.json");
            var inputsTemplate = Path.Combine(outDir, "tx_batch_001.inputs.template.json");
            var summaryPath = Path.Combine(outDir, "tx_batch_001.SUMMARY.json");

            var batch = new Dictionary<string, object?>
            {
                ["network"] = network,
                ["policyId"] = policy,
                ["label_674_metadata"] = new Dictionary<string, object?> { ["purpose"] = "payout", ["batch"] = "001" },
                ["outputs"] = outputs.Take(max).ToArray()
            };
            File.WriteAllText(batchPath, JsonSerializer.Serialize(batch, new JsonSerializerOptions{WriteIndented = true}));

            var inputs = new { inputs = new [] { new { txIn = "<txhash#index>", amountLovelace = "<lovelace>", changeAddress = "<addr>" } } };
            File.WriteAllText(inputsTemplate, JsonSerializer.Serialize(inputs, new JsonSerializerOptions{WriteIndented = true}));

            var summary = new { count = Math.Min(outputs.Count, max), totalAda = outputs.Take(max).Sum(o => Convert.ToDecimal(o["ada"])) };
            File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, new JsonSerializerOptions{WriteIndented = true}));

            var curlPath = Path.Combine(outDir, "submit_batch_001.curl.sh");
            File.WriteAllText(curlPath, "#!/usr/bin/env bash\nset -euo pipefail\nSUBMIT=${SUBMIT_API_URL:-http://localhost:8090}\nFILE=${1:-signed.cbor}\nif [ ! -f \"$FILE\" ]; then echo 'Provide signed CBOR file as arg'; exit 1; fi\ncurl -sS -X POST \"$SUBMIT/api/submit/tx\" --data-binary @\"$FILE\" -H 'Content-Type: application/cbor' | jq .\n");
            TryChmod(curlPath);

            Console.WriteLine($"Wrote: {batchPath}\nWrote: {inputsTemplate}\nWrote: {summaryPath}\nWrote: {curlPath}");
            return 0;
        }
        catch (Exception ex){ Console.Error.WriteLine(ex.Message); return 1; }
    }

    static string Network(Dictionary<string,string> kv){ if(kv.ContainsKey("mainnet")) return "Mainnet"; var m=kv.GetValueOrDefault("testnet-magic")??"1"; return m=="1"?"Preprod":(m=="2"?"Preview":$"Testnet({m})"); }
    static void TryChmod(string p){ try{ System.Diagnostics.Process.Start("chmod", $"755 {p}"); }catch{} }
    static string Req(Dictionary<string,string> kv, string k){ return kv.TryGetValue(k, out var v)? v : throw new ArgumentException($"Missing --{k}"); }
    static Dictionary<string,string> Parse(string[] a){ var kv=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<a.Length;i++){ if(a[i].StartsWith("--")){ var k=a[i][2..]; var v=(i+1<a.Length && !a[i+1].StartsWith("--"))?a[++i]:"true"; kv[k]=v; } } return kv; }
    static List<Dictionary<string,string>> ReadCsv(string path){ var lines=File.ReadAllLines(path).Where(l=>!string.IsNullOrWhiteSpace(l)).ToArray(); if(lines.Length==0) return new(); var hdr=lines[0].Split(',').Select(s=>s.Trim(' ','"')).ToArray(); var list=new List<Dictionary<string,string>>(); foreach(var line in lines.Skip(1)){ var cols=line.Split(','); var row=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<hdr.Length && i<cols.Length;i++){ row[hdr[i]]=cols[i].Trim(' ','"'); } list.Add(row);} return list; }
    static void PrintHelp(){ Console.WriteLine(@"AdaDraft — Draft unsigned ADA payout batches

Usage:
  --ledger cardano-rwa/reports/ada_ledger.csv --out cardano-rwa/out/tx_batches \
  [--policy <id>] [--mainnet|--testnet-magic <N>] [--max-outputs 80]

Artifacts:
  tx_batch_001.json, tx_batch_001.inputs.template.json, tx_batch_001.SUMMARY.json, submit_batch_001.curl.sh

Exit codes: 0 success, 1 error"); }
}
