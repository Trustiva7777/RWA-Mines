#!/usr/bin/env bash
set -euo pipefail

# build_sign_submit.sh <tx_body.json> <signing_key.skey> [submit]
# Requires: cardano-cli, jq

BODY=${1:-}
SKEY=${2:-}
SUBMIT=${SUBMIT_API_URL:-http://localhost:8090}

if [ -z "$BODY" ] || [ -z "$SKEY" ]; then
  echo "Usage: $0 <tx_body.json> <signing_key.skey> [submit]" >&2
  exit 1
fi

if ! command -v cardano-cli >/dev/null; then
  echo "cardano-cli not found; install on signer machine" >&2
  exit 1
fi

TMPDIR=$(mktemp -d)
trap "rm -rf $TMPDIR" EXIT

CBOR=$TMPDIR/signed.cbor

# Placeholder: you would convert JSON body → unsigned tx → sign → $CBOR
echo "Signing placeholder; integrate your offline workflow here." >&2
printf '\x81\x82\x58\x20%s' "00" > "$CBOR"

if [ "${3:-}" = "submit" ]; then
  curl -sS -X POST "$SUBMIT/api/submit/tx" --data-binary @"$CBOR" -H 'Content-Type: application/cbor' | jq .
else
  echo "Signed CBOR at: $CBOR" >&2
fi
