import { execSync } from 'child_process';
import { join, dirname } from 'path';
import { rmSync, readdirSync, readFileSync, writeFileSync, renameSync } from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const config = {
  openApiUrl: join(__dirname, '../../../../../../api-spec/TeensyRom.Api.json'),
  outputDir: join(__dirname, '../src/lib'),
  generator: 'typescript-fetch',
  additionalProps: {
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
    patchApiBarrelFile(config.outputDir);

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
  console.log('Renaming API classes from *Api to *ApiService...');
  const apisDir = join(outputDir, 'apis');
  const files = readdirSync(apisDir);

  for (const file of files) {
    if (!file.endsWith('Api.ts') || file === 'index.ts') continue;

    const fullPath = join(apisDir, file);
    const content = readFileSync(fullPath, 'utf8');
    const match = content.match(/export class (\w+Api)\b/);

    if (!match) continue;

    const originalClassName = match[1];
    const newClassName = originalClassName.replace(/Api$/, 'ApiService');
    const updatedContent = content.replace(
      new RegExp(`\\b${originalClassName}\\b`, 'g'),
      newClassName
    );

    writeFileSync(fullPath, updatedContent, 'utf8');

    const newFilename = file.replace('Api.ts', 'ApiService.ts');
    renameSync(fullPath, join(apisDir, newFilename));
  }

  console.log('Renaming complete!');
}

function patchApiBarrelFile(outputDir) {
  console.log('Patching apis/index.ts to use renamed *ApiService files and class names...');
  const apisDir = join(outputDir, 'apis');
  const indexTsPath = join(apisDir, 'index.ts');

  if (!indexTsPath || !indexTsPath.endsWith('index.ts')) {
    console.warn('apis/index.ts path not valid or missing.');
    return;
  }

  let content = readFileSync(indexTsPath, 'utf8');

  // Replace import/export paths: ./DevicesApi -> ./DevicesApiService
  content = content.replace(/\.\/(\w+)Api'/g, "./$1ApiService'");
  content = content.replace(/\.\/(\w+)Api"/g, './$1ApiService"');

  // Replace class references: DevicesApi -> DevicesApiService
  content = content.replace(/(\w+)Api(\s|$|,|;)/g, '$1ApiService$2');

  writeFileSync(indexTsPath, content, 'utf8');
  console.log('apis/index.ts updated successfully!');
}

main();
