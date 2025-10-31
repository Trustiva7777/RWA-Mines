using System.Text.Json;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var (cmd, kv) = Argx.Parse(args);

        if (cmd == "deploy")
        {
            if (!kv.TryGetValue("contract", out var contractPath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --contract <file> --network <name> --out <file>");
                return 1;
            }

            // Simulate contract deployment
            var contract = new
            {
                name = Path.GetFileNameWithoutExtension(contractPath),
                network = network,
                address = "0x" + Guid.NewGuid().ToString("N").Substring(0, 40),
                deployedAt = DateTime.UtcNow,
                bytecode = "0x608060405234801561001057600080fd5b50d3801561001d57600080fd5b50d2801561002a57600080fd5b5061012f806100396000396000f3fe6080604052348015600f57600080fd5b506004361060285760003560e01c80636d4ce63c14602d575b600080fd5b60336047565b60408051918252519081900360200190f35b60005481565b9091019056fe",
                abi = "[{\"inputs\":[],\"name\":\"getValue\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"}]"
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(contract, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote contract deployment: {outPath}");
            return 0;
        }

        if (cmd == "verify")
        {
            if (!kv.TryGetValue("address", out var address) ||
                !kv.TryGetValue("source", out var sourcePath) ||
                !kv.TryGetValue("network", out var network) ||
                !kv.TryGetValue("out", out var outPath))
            {
                Console.Error.WriteLine("Missing required args: --address <addr> --source <file> --network <name> --out <file>");
                return 1;
            }

            var verification = new
            {
                address = address,
                network = network,
                verified = true,
                timestamp = DateTime.UtcNow,
                sourceHash = "0x" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(sourcePath)))
            };

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(verification, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote contract verification: {outPath}");
            return 0;
        }

        Console.Error.WriteLine("Usage:\n  deploy --contract <file> --network <name> --out <file>\n  verify --address <addr> --source <file> --network <name> --out <file>");
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