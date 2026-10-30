import { cp } from 'node:fs/promises';

const environment = process.argv[2];

if (!['local', 'stage', 'prod'].includes(environment)) {
  throw new Error(
    'Invalid environment. Use: local, stage, or prod.'
  );
}

await cp(
  `config/app-config.${environment}.json`,
  'public/config/app-config.json'
);

console.log(`Using ${environment} configuration.`);