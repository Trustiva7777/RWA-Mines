import xrpl from 'xrpl';

const url = process.env.XRPL_JSON_RPC || 'https://s.altnet.rippletest.net:51234';

async function main() {
  console.log(`XRPL issuer dev starting → ${url}`);
  const client = new xrpl.Client(url);
  await client.connect();
  console.log('Connected to XRPL');
  await client.disconnect();
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
