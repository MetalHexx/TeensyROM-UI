import { execSync } from 'child_process';

// Get the project name from the file path
function getProjectName(filePath) {
  const parts = filePath.split('/');
  if (parts[0] === 'apps') {
    return parts[1];
  }
  if (parts[0] === 'libs') {
    return parts[1];
  }
  return null;
}

// Get all staged files
const stagedFiles = process.argv.slice(2);

// Group files by project
const filesByProject = stagedFiles.reduce((acc, file) => {
  const project = getProjectName(file);
  if (project) {
    acc[project] = acc[project] || [];
    acc[project].push(file);
  }
  return acc;
}, {});

// Run lint for each project that has staged files
Object.keys(filesByProject).forEach((project) => {
  console.log(`Linting project: ${project}`);
  try {
    execSync(`nx lint ${project}`, { stdio: 'inherit' });
  } catch {
    process.exit(1);
  }
});
