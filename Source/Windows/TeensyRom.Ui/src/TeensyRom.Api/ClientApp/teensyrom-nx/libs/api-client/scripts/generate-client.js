import { execSync } from 'child_process';
import { join, dirname } from 'path';
import { rmSync, readdirSync, readFileSync, writeFileSync, renameSync } from 'fs';
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
    withSeparateModelsAndApi: true,
  },
};

function main() {
  try {
    const additionalProps = {
      ...config.additionalProps,
      withSeparateModelsAndApi: config.additionalProps.withSeparateModelsAndApi.toString(),
    };
    const additionalPropsStr = buildAdditionalPropsString(additionalProps);

    cleanOutputDirectory(config.outputDir);
    generateOpenApiClient(
      config.openApiUrl,
      config.outputDir,
      config.generator,
      additionalPropsStr
    );
    renameApiServiceClasses(config.outputDir);

    console.log('OpenAPI client generation completed successfully!');
  } catch (error) {
    console.error('Error generating OpenAPI client:', error);
    process.exit(1);
  }
}

function buildAdditionalPropsString(additionalProps) {
  return Object.entries(additionalProps)
    .map(([key, value]) => `${key}=${value}`)
    .join(',');
}

function cleanOutputDirectory(outputDir) {
  console.log('Cleaning output directory...');
  rmSync(outputDir, { recursive: true, force: true });
}

function generateOpenApiClient(openApiUrl, outputDir, generator, additionalPropsStr) {
  console.log('Generating OpenAPI client...');
  const command = `openapi-generator-cli generate \
    -i ${openApiUrl} \
    -g ${generator} \
    -o ${outputDir} \
    --skip-validate-spec \
    --additional-properties=${additionalPropsStr}`;
  execSync(command, { stdio: 'inherit' });
}

function renameApiServiceClasses(outputDir) {
  console.log('Renaming API service classes from *Service to *ApiService...');
  const apiDir = join(outputDir, 'api');
  const files = readdirSync(apiDir);

  for (const file of files) {
    if (!file.endsWith('.service.ts')) continue;

    const fullPath = join(apiDir, file);
    const content = readFileSync(fullPath, 'utf8');
    const match = content.match(/export class (\w+Service)\b/);

    if (!match) continue;

    const originalClassName = match[1];
    const newClassName = originalClassName.replace(/Service$/, 'ApiService');
    const updatedContent = content.replace(
      new RegExp(`\\b${originalClassName}\\b`, 'g'),
      newClassName
    );

    writeFileSync(fullPath, updatedContent, 'utf8');

    const newFilename = file.replace('.service.ts', '.api.service.ts');
    renameSync(fullPath, join(apiDir, newFilename));
  }

  console.log('Renaming complete!');
}

main();
