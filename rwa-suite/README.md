# RWA Suite - Multi-Chain Real World Asset Platform

A production-ready monorepo for deploying Real World Assets (RWAs) across multiple blockchains including Cardano, EVM, XRPL, Solana, and Cosmos.

## Features

- **Multi-Chain Support**: Deploy RWAs on Cardano, EVM, XRPL, Solana, and Cosmos
- **Compliance & Attestation**: Built-in compliance checking and cryptographic attestation
- **Operator Tools**: Complete toolkit for asset issuance, payout planning, and transaction management
- **Security**: AES-256-GCM encrypted seeds, hash-based attestations, deterministic operations
- **CI/CD**: Automated testing and deployment pipelines
- **VS Code Integration**: One-click operator workflows

## Quick Start

1. Clone the repository
2. Run `pnpm bootstrap` to install dependencies
3. Use VS Code tasks for operator workflows

## Architecture

### Cardano Tools (.NET)
- **PolicyTool**: Create and manage minting policies with time locks
- **HashAttest**: Generate manifests and cryptographic attestations
- **SeedGen**: Secure seed generation with AES-256-GCM encryption
- **PayoutPlan**: Deterministic payout planning from manifests
- **AdaDraft**: Transaction drafting with policy validation
- **Health**: System health checks and validation

### EVM Tools (.NET)
- **ContractTool**: Deploy and verify smart contracts
- **TokenTool**: Mint and transfer ERC-20 tokens

### XRPL Tools (.NET)
- **LedgerTool**: Submit transactions and check status
- **TokenTool**: Issue and transfer XRPL tokens

### Solana Tools (.NET)
- **ProgramTool**: Deploy and verify Solana programs
- **TokenTool**: Create and mint SPL tokens

### Cosmos Tools (.NET)
- **ModuleTool**: Store and instantiate CosmWasm modules
- **TokenTool**: Issue and transfer Cosmos tokens

### Compliance & Proofs (TypeScript)
- Automated compliance checking
- Proof generation and validation
- Sanctions screening integration

### Scripts
- **Signer**: Secure transaction signing with encrypted seeds

## Detailed Chain Documentation

See [`docs/CHAIN_DOCUMENTATION.md`](docs/CHAIN_DOCUMENTATION.md) for comprehensive information about:
- Chain-specific directory structures
- Operator workflows for each blockchain
- Security considerations
- Development patterns

## Operator Flow

### Cardano
1. **Lock**: Plan policy locks with PolicyTool
2. **Attest**: Generate attestations with HashAttest
3. **Ledger**: Create payout plans with PayoutPlan
4. **Draft**: Generate transaction drafts with AdaDraft

Use the "Cardano: Full Operator Flow" VS Code task for automated execution.

### EVM
1. **Deploy**: Deploy contracts with ContractTool
2. **Verify**: Verify contracts on block explorers
3. **Mint**: Mint tokens with TokenTool
4. **Transfer**: Transfer tokens between addresses

### XRPL
1. **Submit**: Submit transactions with LedgerTool
2. **Status**: Check transaction status
3. **Issue**: Issue tokens with TokenTool
4. **Transfer**: Transfer tokens between addresses

### Solana
1. **Deploy**: Deploy programs with ProgramTool
2. **Verify**: Verify program deployments
3. **Create**: Create tokens with TokenTool
4. **Mint**: Mint tokens to addresses

### Cosmos
1. **Store**: Store modules with ModuleTool
2. **Instantiate**: Instantiate contracts
3. **Issue**: Issue tokens with TokenTool
4. **Transfer**: Transfer tokens between addresses

## Security

- All seeds encrypted with AES-256-GCM
- Hash-only attestations (no sensitive data)
- Deterministic operations for auditability
- .gitignore blocks sensitive files

## Development

- .NET 9.0 for all chain tools (Cardano, EVM, XRPL, Solana, Cosmos)
- Node.js/TypeScript for compliance packages
- Hardhat/Foundry for EVM development
- GitHub Actions for CI/CD
- VS Code tasks for development workflows