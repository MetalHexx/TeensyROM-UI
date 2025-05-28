import { execSync } from 'child_process';
import { join, dirname } from 'path';
import { rmSync } from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const config = {
  openApiUrl: 'http://localhost:5168/openapi/v1.json',
  outputDir: join(__dirname, '../src/lib'),
  generator: 'typescript-angular',
  additionalProps: {
    ngVersion: '19',
    providedIn: 'any',
    withSeparateModelsAndApi: true
  }
};

const additionalPropsStr = Object.entries(config.additionalProps)
  .map(([key, value]) => `${key}=${value}`)
  .join(',');

try {
  console.log('Cleaning output directory...');
  rmSync(config.outputDir, { recursive: true, force: true });

  console.log('Generating OpenAPI client...');
  const command = `openapi-generator-cli generate \
    -i ${config.openApiUrl} \
    -g ${config.generator} \
    -o ${config.outputDir} \
    --skip-validate-spec \
    --additional-properties=${additionalPropsStr}`;

  execSync(command, { stdio: 'inherit' });

  console.log('OpenAPI client generation completed successfully!');
} catch (error) {
  console.error('Error generating OpenAPI client:', error);
  process.exit(1);
}
