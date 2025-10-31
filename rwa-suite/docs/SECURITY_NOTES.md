# Security Notes

- Never commit mnemonics or raw seeds. Prefer hardware wallets on Mainnet.
- Optional `seed.enc` is encrypted-at-rest; do not store passphrases in plaintext.
- Rotate mint and admin keys periodically.
- Use least-privilege policies for deployment and CI.
- Treat allowlists and attestations as sensitive audit artifacts.
