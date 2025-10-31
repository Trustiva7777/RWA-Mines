using System.Net.Http;
using System.Text.Json;
using NJsonSchema;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help")) { PrintHelp(); return 0; }
        try
        {
            var cmd = args[0].ToLowerInvariant();
            var kv = Parse(args[1..]);
            return cmd switch
            {
                "ping" => await Ping(kv),
                "validate" => await Validate(kv),
                _ => Err("Unknown command. Use --help.")
            };
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }

    static async Task<int> Ping(Dictionary<string,string> kv)
    {
        var urls = new [] {
            kv.GetValueOrDefault("node") ?? Environment.GetEnvironmentVariable("NODE_URL"),
            kv.GetValueOrDefault("ogmios") ?? Environment.GetEnvironmentVariable("OGMIOS_URL"),
            kv.GetValueOrDefault("submit") ?? Environment.GetEnvironmentVariable("SUBMIT_API_URL")
        }.Where(u => !string.IsNullOrWhiteSpace(u)).ToArray();
        if (urls.Length == 0) { Console.WriteLine("No endpoints given."); return 0; }
        using var http = new HttpClient();
        foreach (var u in urls)
        {
            try
            {
                var resp = await http.GetAsync(u!);
                Console.WriteLine($"{u} → {(int)resp.StatusCode}");
            }
            catch (Exception ex) { Console.WriteLine($"{u} → ERROR: {ex.Message}"); }
        }
        return 0;
    }

    static async Task<int> Validate(Dictionary<string,string> kv)
    {
        var schemaPath = Req(kv, "schema");
        var jsonPath = Req(kv, "json");
        var schema = await JsonSchema.FromFileAsync(schemaPath);
        var data = await File.ReadAllTextAsync(jsonPath);
        var errors = schema.Validate(data);
        if (errors.Count == 0) { Console.WriteLine("VALID"); return 0; }
        foreach (var e in errors) Console.WriteLine($"{e.Path}: {e.Kind}");
        return 1;
    }

    static int Err(string m){ Console.Error.WriteLine(m); return 1; }
    static string Req(Dictionary<string,string> kv, string k){ return kv.TryGetValue(k, out var v)? v : throw new ArgumentException($"Missing --{k}"); }
    static Dictionary<string,string> Parse(string[] a){ var kv=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase); for(int i=0;i<a.Length;i++){ if(a[i].StartsWith("--")){ var k=a[i][2..]; var v=(i+1<a.Length && !a[i+1].StartsWith("--"))?a[++i]:"true"; kv[k]=v; }} return kv; }
    static void PrintHelp(){ Console.WriteLine(@"Health — Endpoint ping and schema validation

Usage:
  ping [--node <url>] [--ogmios <url>] [--submit <url>]
  validate --schema cardano-rwa/docs/attestation.schema.json --json cardano-rwa/docs/attestation.Preprod.json

Exit codes: 0 success, 1 error"); }
}
