import baseConfig from '../../eslint.config.mjs';

export default [
    ...baseConfig,
    {
        ignores: [
          // Ignore everything in the generated OpenAPI output
          'src/lib/api/**/*',
          'src/lib/model/**/*',
          'src/lib/api.module.ts',
          'src/lib/configuration.ts',
          'src/lib/encoder.ts',
          'src/lib/param.ts',
          'src/lib/variables.ts',
          'src/lib/api.base.service.ts',
          'src/lib/README.md',
          'src/lib/git_push.sh'
        ]
    }
];
