# Multi-Chain RWA Documentation

This document provides detailed information about deploying Real World Assets (RWAs) across multiple blockchains using the RWA Suite.

## Supported Chains

### Cardano
**Tools**: PolicyTool, HashAttest, SeedGen, PayoutPlan, AdaDraft, Health
**Network Support**: Mainnet, Preprod, Preview
**Key Features**:
- Time-locked minting policies
- Cryptographic attestations
- Deterministic payout planning
- Transaction batching

**Directory Structure**:
```
chains/cardano/
├── docs/
│   ├── sha256-manifest.json    # File integrity manifest
│   ├── allowlist.csv          # Approved files list
│   └── token/
│       └── attestation.*.json # Network-specific attestations
├── policy/
│   └── policy.json            # Minting policy configuration
├── reports/                   # Generated reports
├── out/                       # Transaction outputs
└── wallet/                    # Encrypted wallet seeds
```

### EVM (Ethereum Virtual Machine)
**Tools**: ContractTool, TokenTool
**Network Support**: Mainnet, Sepolia, Polygon, Arbitrum, Optimism
**Key Features**:
- Smart contract deployment and verification
- ERC-20 token minting and transfers
- Multi-chain compatibility

**Directory Structure**:
```
chains/evm/
├── docs/
│   ├── sha256-manifest.json    # File integrity manifest
│   └── token/
│       └── attestation.*.json # Network-specific attestations
├── contracts/                 # Solidity contracts
├── hardhat/                   # Hardhat configuration
├── foundry/                   # Foundry configuration
├── tools/                     # .NET operator tools
└── out/                       # Deployment outputs
```

### XRPL (XRP Ledger)
**Tools**: LedgerTool, TokenTool
**Network Support**: Mainnet, Testnet, Devnet
**Key Features**:
- Transaction submission and status monitoring
- IOU token issuance and transfers
- Trustline management

**Directory Structure**:
```
chains/xrpl/
├── docs/
│   ├── sha256-manifest.json    # File integrity manifest
│   └── token/
│       └── attestation.*.json # Network-specific attestations
├── issuer/                    # Token issuer application
├── tools/                     # .NET operator tools
├── tx/                        # Transaction files
└── out/                       # Transaction outputs
```

### Solana
**Tools**: ProgramTool, TokenTool
**Network Support**: Mainnet-beta, Devnet, Testnet
**Key Features**:
- Program deployment and verification
- SPL token creation and minting
- High-throughput operations

**Directory Structure**:
```
chains/solana/
├── docs/
│   ├── sha256-manifest.json    # File integrity manifest
│   └── token/
│       └── attestation.*.json # Network-specific attestations
├── programs/                  # Solana programs (Rust)
├── scripts/                   # Deployment scripts
├── tools/                     # .NET operator tools
└── out/                       # Deployment outputs
```

### Cosmos
**Tools**: ModuleTool, TokenTool
**Network Support**: Cosmoshub, Osmosis, Juno, Terra, Kava
**Key Features**:
- CosmWasm module storage and instantiation
- IBC-compatible token operations
- Interoperability focus

**Directory Structure**:
```
chains/cosmos/
├── docs/
│   ├── sha256-manifest.json    # File integrity manifest
│   └── token/
│       └── attestation.*.json # Network-specific attestations
├── contracts/                 # CosmWasm contracts (Rust)
├── module-rwa/               # RWA module
├── tools/                     # .NET operator tools
└── out/                       # Deployment outputs
```

## Common Patterns

### Manifest Files
All chains use SHA256 manifest files to ensure file integrity:
```json
{
  "README.md": "a1b2c3d4...",
  "contracts/RWA.sol": "b2c3d4e5..."
}
```

### Attestation Files
Chain-specific attestation files contain deployment metadata:
```json
{
  "contractAddress": "0x742d...",
  "network": "sepolia",
  "manifestSha256": "a1b2c3d4...",
  "timestamp": "2025-10-31T01:00:00.0000000+00:00"
}
```

### Operator Workflows

#### EVM Deployment Flow
1. **Deploy**: `ContractTool deploy --network sepolia`
2. **Verify**: `ContractTool verify --deployment contract_deployment.json`
3. **Mint**: `TokenTool mint --contract 0x... --amount 1000`
4. **Transfer**: `TokenTool transfer --contract 0x... --to 0x... --amount 500`

#### XRPL Token Flow
1. **Submit**: `LedgerTool submit --network testnet --tx-json tx.json`
2. **Status**: `LedgerTool status --tx-hash ABC123...`
3. **Issue**: `TokenTool issue --currency USD --issuer rHb9... --amount 1000`
4. **Transfer**: `TokenTool transfer --currency USD --to rRecipient... --amount 500`

#### Solana Program Flow
1. **Deploy**: `ProgramTool deploy --network devnet --program-path program.so`
2. **Verify**: `ProgramTool verify --deployment program_deployment.json`
3. **Create**: `TokenTool create --decimals 6`
4. **Mint**: `TokenTool mint --token So111... --amount 1000 --to recipient...`

#### Cosmos Module Flow
1. **Store**: `ModuleTool store --network osmosis --wasm-path contract.wasm`
2. **Instantiate**: `ModuleTool instantiate --code-id 1234 --init-msg "{}"`
3. **Issue**: `TokenTool issue --denom uatom --amount 1000 --to cosmos1...`
4. **Transfer**: `TokenTool transfer --denom uatom --amount 500 --to cosmos1...`

## Security Considerations

- All sensitive data (seeds, private keys) are encrypted with AES-256-GCM
- Manifests ensure file integrity across deployments
- Attestations provide cryptographic proof of compliance
- Deterministic operations enable auditability

## Development

Each chain follows consistent patterns:
- .NET 9.0 tools for operator workflows
- JSON schemas for artifact validation
- SHA256 manifests for integrity
- Chain-specific attestation formats

Use VS Code tasks for streamlined operator workflows across all chains.