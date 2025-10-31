using System.Security.Cryptography;
using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "check")
        {
            if (!kv.TryGetValue("manifest", out var manifestPath) ||
                !kv.TryGetValue("attestation", out var attestationPath))
            {
                Console.Error.WriteLine("Missing required args: --manifest <file> --attestation <file> [--policy <file>]");
                return 1;
            }

            var issues = new List<string>();

            // Validate manifest
            if (!File.Exists(manifestPath))
            {
                issues.Add($"Manifest file not found: {manifestPath}");
            }
            else
            {
                try
                {
                    var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(manifestPath));
                    if (manifest == null) issues.Add("Invalid manifest JSON");
                }
                catch
                {
                    issues.Add("Failed to parse manifest JSON");
                }
            }

            // Validate attestation
            if (!File.Exists(attestationPath))
            {
                issues.Add($"Attestation file not found: {attestationPath}");
            }
            else
            {
                try
                {
                    var attestation = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(attestationPath));
                    if (attestation == null) issues.Add("Invalid attestation JSON");
                }
                catch
                {
                    issues.Add("Failed to parse attestation JSON");
                }
            }

            // Optional policy validation
            if (kv.TryGetValue("policy", out var policyPath))
            {
                if (!File.Exists(policyPath))
                {
                    issues.Add($"Policy file not found: {policyPath}");
                }
                else
                {
                    try
                    {
                        var policy = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(policyPath));
                        if (policy == null) issues.Add("Invalid policy JSON");
                    }
                    catch
                    {
                        issues.Add("Failed to parse policy JSON");
                    }
                }
            }

            // Output status
            var status = issues.Count == 0 ? "healthy" : "unhealthy";
            var result = new
            {
                status = status,
                timestamp = DateTime.UtcNow,
                issues = issues.ToArray()
            };

            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return issues.Count == 0 ? 0 : 1;
        }

        Console.Error.WriteLine("Usage:\n  check --manifest <file> --attestation <file> [--policy <file>]");
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