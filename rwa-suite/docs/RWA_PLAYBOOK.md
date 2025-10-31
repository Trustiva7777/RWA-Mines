# RWA Playbook

This playbook outlines a pragmatic operator path to bring a compliant RWA live across multiple chains.

## Legal + Structure
- Establish appropriate wrapper (SPV/trust/etc.)
- Define redemption, reporting, and investor rights

## Proofs and Attestations
- Maintain document set in `docs/` and generate `sha256-manifest.json`
- Canonicalize allowlist and publish hash
- Record `beforeSlot` (Cardano) and network in attestation

## Compliance + Gates
- Enforce allowlist, sanctions, and lockups across chain adapters
- Keep a deterministic sanctions mock for local testing

## Operations
- SeedGen generates mnemonics for testnets only; hardware signing for production
- Payout planning → ADA/XRP ledgers → batch drafts
- Keep audit trails: manifest SHAs, attestation JSON, and distribution reports
