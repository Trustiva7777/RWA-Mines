# RWA Suite JSON Schemas

This directory contains JSON schemas for validating artifacts produced by the RWA Suite tools across all supported blockchains.

## Core Schemas

### Cardano
- `attestation.schema.json` - Cryptographic attestations for Cardano policies
- `manifest.schema.json` - File manifests with SHA256 hashes
- `policy.schema.json` - Cardano minting policy configurations

## Chain-Specific Schemas

### EVM
- `evm-contract-deployment.schema.json` - Smart contract deployment artifacts
- `evm-contract-verification.schema.json` - Contract verification results
- `evm-token-operation.schema.json` - ERC-20 token minting and transfers

### XRPL
- `xrpl-transaction-submission.schema.json` - Transaction submission artifacts
- `xrpl-transaction-status.schema.json` - Transaction status check results
- `xrpl-token-operation.schema.json` - XRPL token issuance and transfers

### Solana
- `solana-program-deployment.schema.json` - Program deployment artifacts
- `solana-program-verification.schema.json` - Program verification results
- `solana-token-operation.schema.json` - SPL token creation and minting

### Cosmos
- `cosmos-module-storage.schema.json` - WASM module storage artifacts
- `cosmos-module-instantiation.schema.json` - Contract instantiation results
- `cosmos-token-operation.schema.json` - Cosmos token issuance and transfers

## Usage

Use these schemas to validate JSON outputs from RWA Suite tools:

```bash
# Validate an attestation file
npx ajv validate -s schemas/attestation.schema.json -d chains/cardano/docs/token/attestation.Preprod.json

# Validate a contract deployment
npx ajv validate -s schemas/evm-contract-deployment.schema.json -d chains/evm/out/contract_deployment.json
```

## Schema Standards

All schemas follow JSON Schema Draft 2020-12 and include:
- Comprehensive property definitions
- Type validation
- Pattern matching for addresses/hashes
- Required field specifications
- Descriptive documentation