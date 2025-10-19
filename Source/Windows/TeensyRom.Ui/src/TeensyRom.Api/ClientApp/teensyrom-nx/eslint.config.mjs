import nx from '@nx/eslint-plugin';

export default [
  ...nx.configs['flat/base'],
  ...nx.configs['flat/typescript'],
  ...nx.configs['flat/javascript'],
  {
    ignores: ['**/dist', '**/vite.config.*.timestamp*', '**/vitest.config.*.timestamp*'],
  },
  {
    files: ['**/*.ts', '**/*.tsx', '**/*.js', '**/*.jsx'],
    rules: {
      '@nx/enforce-module-boundaries': [
        'error',
        {
          enforceBuildableLibDependency: true,
          allow: ['^.*/eslint(\\.base)?\\.config\\.[cm]?js$'],
          depConstraints: [
            // Domain Layer - No dependencies allowed (pure business logic)
            {
              sourceTag: 'scope:domain',
              onlyDependOnLibsWithTags: [],
            },
            // Application Layer - Can only depend on domain and shared utilities
            {
              sourceTag: 'scope:application',
              onlyDependOnLibsWithTags: ['scope:domain', 'scope:shared'],
            },
            // Infrastructure Layer - Can depend on domain, shared utilities, and api-client
            {
              sourceTag: 'scope:infrastructure',
              onlyDependOnLibsWithTags: ['scope:domain', 'scope:shared', 'scope:data-access'],
            },
            // Features Layer - Can depend on application, domain, and shared UI
            {
              sourceTag: 'scope:features',
              onlyDependOnLibsWithTags: ['scope:application', 'scope:domain', 'scope:shared'],
            },
            // Shared libraries - Can depend on each other and domain
            {
              sourceTag: 'scope:shared',
              onlyDependOnLibsWithTags: ['scope:shared', 'scope:domain'],
            },
            // App Layer - Composition root: can depend on everything
            {
              sourceTag: 'scope:app',
              onlyDependOnLibsWithTags: [
                'scope:domain',
                'scope:features',
                'scope:application',
                'scope:infrastructure',
                'scope:shared',
                'scope:app',
              ],
            },
            // Prevent features from importing each other (feature isolation)
            {
              sourceTag: 'feature:device',
              notDependOnLibsWithTags: ['feature:player'],
            },
            {
              sourceTag: 'feature:player',
              notDependOnLibsWithTags: ['feature:device'],
            },
          ],
        },
      ],
    },
  },
  {
    files: [
      '**/*.ts',
      '**/*.tsx',
      '**/*.cts',
      '**/*.mts',
      '**/*.js',
      '**/*.jsx',
      '**/*.cjs',
      '**/*.mjs',
    ],
    // Override or add rules here
    rules: {},
  },
];
