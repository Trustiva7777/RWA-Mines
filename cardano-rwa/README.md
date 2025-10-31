# Cardano RWA — End-to-End Guide

This package contains all operator artifacts for a Cardano-native RWA flow. Tools live in `../tools/` and are invoked via VS Code tasks or direct `dotnet run`.

## Prereqs
- .NET 9 SDK
- Optional: `cardano-cli`, `jq` for offline signing (on signer machine)

## Environment
Copy `.env.examples` to `.env.preprod` or `.env.mainnet` and fill:
- `OGMIOS_URL`
- `SUBMIT_API_URL`
- `NODE_URL` (optional health)

## Flow (Preprod default)
1. Wallet: generate encrypted seed and addresses (no mnemonic stored)
2. Policy: plan lock slot using current slot
3. Proofs: build docs manifest
4. Attestation: combine policy/network/manifest/allowlist/beforeSlot
5. Payouts: plan → ADA ledger
6. Distribute: draft unsigned batches + inputs template + metadata
7. Signer bundle: export for offline signing box
8. Submit: curl stub to submit-api and status check

See `.vscode/tasks.json` for one-click tasks.