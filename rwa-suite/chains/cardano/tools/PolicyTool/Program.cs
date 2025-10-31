using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace PolicyToolApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 0;
            }

            string command = args[0];
            switch (command)
            {
                case "create":
                    return await HandleCreate(args.Skip(1).ToArray());
                case "plan-lock":
                    return await HandlePlanLock(args.Skip(1).ToArray());
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    return 1;
            }
        }

        static async Task<int> HandleCreate(string[] args)
        {
            string @out = null, vkeyHash = null, network = "Preprod";
            int beforeSlot = 0;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--out":
                        if (i + 1 < args.Length) @out = args[++i];
                        break;
                    case "--vkey-hash":
                        if (i + 1 < args.Length) vkeyHash = args[++i];
                        break;
                    case "--before-slot":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out beforeSlot)) { }
                        break;
                    case "--network":
                        if (i + 1 < args.Length) network = args[++i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(@out) || string.IsNullOrEmpty(vkeyHash) || beforeSlot == 0)
            {
                Console.WriteLine("Missing required arguments for create: --out, --vkey-hash, --before-slot");
                return 1;
            }

            return await CreatePolicy(@out, vkeyHash, beforeSlot, network);
        }

        static async Task<int> HandlePlanLock(string[] args)
        {
            string @out = null, network = "Preprod";
            int days = 0;
            long currentSlot = 0;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--out":
                        if (i + 1 < args.Length) @out = args[++i];
                        break;
                    case "--days":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out days)) { }
                        break;
                    case "--current-slot":
                        if (i + 1 < args.Length && long.TryParse(args[++i], out currentSlot)) { }
                        break;
                    case "--network":
                        if (i + 1 < args.Length) network = args[++i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(@out) || days == 0 || currentSlot == 0)
            {
                Console.WriteLine("Missing required arguments for plan-lock: --out, --days, --current-slot");
                return 1;
            }

            return await PlanLock(@out, days, currentSlot, network);
        }

        static void ShowHelp()
        {
            Console.WriteLine("Cardano PolicyTool: create/lock policy, compute lock plan, output policy.json and policy.sha256.json");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  create     Create a new policy.json and policy.sha256.json");
            Console.WriteLine("             --out <path>          Output base path (required)");
            Console.WriteLine("             --vkey-hash <hash>    Verification key hash (hex, required)");
            Console.WriteLine("             --before-slot <slot>  Slot before which policy expires (required)");
            Console.WriteLine("             --network <net>       Network (Mainnet, Preprod, Preview, default: Preprod)");
            Console.WriteLine();
            Console.WriteLine("  plan-lock  Compute lock plan for policy (outputs .json and .beforeSlot)");
            Console.WriteLine("             --out <path>          Output base path (required)");
            Console.WriteLine("             --days <days>         Days until lock (required)");
            Console.WriteLine("             --current-slot <slot> Current slot number (required)");
            Console.WriteLine("             --network <net>       Network (Mainnet, Preprod, Preview, default: Preprod)");
            Console.WriteLine();
            Console.WriteLine("Use --help or -h for this message.");
        }

        static async Task<int> CreatePolicy(string @out, string vkeyHash, int beforeSlot, string network)
        {
            object[] scripts = new object[] {
                new {
                    type = "before",
                    slot = beforeSlot
                },
                new {
                    type = "sig",
                    keyHash = vkeyHash
                }
            };
            var policy = new {
                type = "all",
                scripts,
                network
            };
            var json = JsonSerializer.Serialize(policy, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(@out, json);
            var sha256 = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            var shaObj = new { sha256 = BitConverter.ToString(sha256).Replace("-", "").ToLowerInvariant() };
            var shaPath = Path.ChangeExtension(@out, ".sha256.json");
            await File.WriteAllTextAsync(shaPath, JsonSerializer.Serialize(shaObj, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Wrote: {@out}\nWrote: {shaPath}");
            return 0;
        }

        static async Task<int> PlanLock(string @out, int days, long currentSlot, string network)
        {
            // Cardano: 1 day = 86400 seconds, 1 slot = 1s (mainnet/preprod/preview)
            var beforeSlot = currentSlot + days * 86400;
            var plan = new { currentSlot, days, beforeSlot, network };
            await File.WriteAllTextAsync(@out, JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true }));
            var beforeSlotPath = Path.ChangeExtension(@out, ".beforeSlot");
            await File.WriteAllTextAsync(beforeSlotPath, beforeSlot.ToString());
            Console.WriteLine($"Wrote: {@out}\nWrote: {beforeSlotPath}");
            return 0;
        }
    }
}

// --- End ---
