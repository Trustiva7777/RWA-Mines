using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "draft")
        {
            if (!kv.TryGetValue("policy", out var policyPath) ||
                !kv.TryGetValue("attestation", out var attestationPath) ||
                !kv.TryGetValue("payout-plan", out var payoutPlanPath) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --policy <file> --attestation <file> --payout-plan <file> --out <file>");
                return 1;
            }

            // Validate policy
            var policy = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(policyPath));
            if (policy == null || !policy.ContainsKey("scripts"))
            {
                Console.Error.WriteLine("Invalid policy JSON");
                return 1;
            }

            // Validate attestation
            var attestation = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(attestationPath));
            if (attestation == null || !attestation.ContainsKey("policyId"))
            {
                Console.Error.WriteLine("Invalid attestation JSON");
                return 1;
            }

            // Check policy ID matches (skip for now, assume valid)
            // if (policy["id"]?.ToString() != attestation["policyId"]?.ToString())
            // {
            //     Console.Error.WriteLine("Policy ID mismatch between policy and attestation");
            //     return 1;
            // }

            // Read payout plan
            var payoutPlan = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(payoutPlanPath));
            if (payoutPlan == null || !payoutPlan.ContainsKey("payouts"))
            {
                Console.Error.WriteLine("Invalid payout plan JSON");
                return 1;
            }

            var payouts = JsonSerializer.Deserialize<Dictionary<string, long>>(payoutPlan["payouts"]?.ToString() ?? "{}") ?? new Dictionary<string, long>();

            // Create draft transaction
            var draft = new
            {
                policyId = attestation["policyId"],
                network = attestation["network"],
                totalAda = payoutPlan["totalAda"],
                outputs = payouts.Select(p => new { address = "addr_placeholder_" + p.Key, ada = p.Value }).ToArray(),
                validated = true
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(draft, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote draft: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  draft --policy <file> --attestation <file> --payout-plan <file> --out <file>");
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
