# RWA Suite

A production-oriented, multi-chain RWA monorepo spanning Cardano, EVM, XRPL, Solana, and Cosmos, with shared compliance, attestations, and payout tooling.

## Quick start

1. Copy environment: `.env.example` → `.env` and fill as needed.
2. In VS Code, run task: "Bootstrap" to install workspace deps.
3. Cardano operator flow:
   - "Cardano: Proofs → manifest (.NET)"
   - "Cardano: Plan lock (.NET with current slot)"
   - "Cardano: Attest (auto)"
   - "Cardano: Draft ADA tx batches (.NET)"
4. EVM: run Hardhat or Foundry in `chains/evm/`.
5. XRPL: start issuer dev server in `chains/xrpl/issuer`.

Artifacts target paths:
- `chains/cardano/docs/sha256-manifest.json`
- `chains/cardano/reports/lock_plan.json` and `chains/cardano/reports/lock_plan.beforeSlot`
- `chains/cardano/docs/token/attestation.<Network>.json`
- `chains/cardano/reports/ada_ledger.csv`
- `chains/cardano/out/tx_batches/tx_batch_001.json`

## Notes
- No secrets are committed. Use encrypted `seed.enc` (optional) from SeedGen, or hardware wallets for Mainnet.
- Canonical allowlist hashes and attestations are produced to ensure integrity.

See `docs/MAINNET_READINESS_CHECKLIST.md`, `RWA_PLAYBOOK.md`, and `SECURITY_NOTES.md` for deeper context.