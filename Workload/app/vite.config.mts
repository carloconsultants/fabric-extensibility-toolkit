import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import path from 'path';
// Note: avoid importing CommonJS modules directly in the config. Use createRequire.
import { createRequire } from 'node:module';
import dotenv from 'dotenv';

// Load env from .env.* when running via `env-cmd -f .env.dev vite` or similar.
dotenv.config();

// Keep the same port/host Fabric DevGateway expects
const DEV_HOST = '127.0.0.1';
const DEV_PORT = 60006;

// Root is now the current directory (app/) since we run from here
const appRoot = __dirname;
const outDir = path.resolve(__dirname, '../build/Frontend');

export default defineConfig({
  root: appRoot,
  plugins: [
    react(),
    tsconfigPaths(),
    // Copy static files not handled by bundling (e.g., web.config) on build
    viteStaticCopy({
      targets: [
        { src: path.resolve(appRoot, 'web.config'), dest: '' },
      ],
    }),
    // Register existing Express-based dev APIs under the Vite dev server
    {
      name: 'fabric-dev-apis',
      configureServer(server) {
  const require = createRequire(import.meta.url);
        // Mirror the headers/CORS behavior from webpack-dev-server setup
        server.middlewares.use((req, res, next) => {
          res.setHeader('Access-Control-Allow-Origin', '*');
          res.setHeader('Access-Control-Allow-Methods', 'GET, PUT, POST, DELETE, OPTIONS');
          res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, Content-Length, X-Requested-With');
          if (req.method === 'OPTIONS') {
            res.statusCode = 204;
            res.end();
            return;
          }
          next();
        });

        try {
          // Load the existing routers and register them against Vite's connect app
          const { registerDevServerApis } = require('./devServer');
          // Create a shim object exposing only the 'use' signature that registerDevServerApis expects
          const appShim = {
            use: (...args: any[]) => {
              // @ts-ignore - connect instance is compatible with express middleware signature
              server.middlewares.use.apply(server.middlewares, args as any);
              return appShim;
            }
          } as unknown as { use: (...handlers: any[]) => any };

          registerDevServerApis(appShim as any);

          console.log('*********************************************************************');
          console.log('****               Vite server listening on port 60006           ****');
          console.log('****   You can now override the Fabric manifest with your own.   ****');
          console.log('*********************************************************************');
        } catch (e) {
          console.error('Failed to register Fabric dev APIs on Vite server:', e);
        }
      },
    },
  ],
  publicDir: path.resolve(appRoot, 'assets'),
  server: {
    host: DEV_HOST,
    port: DEV_PORT,
    strictPort: true,
    open: false,
    cors: true,
  },
  build: {
    outDir,
    emptyOutDir: true,
    sourcemap: true,
  },
  resolve: {
    extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json'],
  },
  define: {
    'process.env.DEV_AAD_CONFIG_FE_APPID': JSON.stringify(process.env.FRONTEND_APPID ?? ''),
    'process.env.DEV_AAD_CONFIG_BE_APPID': JSON.stringify(process.env.BACKEND_APPID ?? ''),
    'process.env.DEV_AAD_CONFIG_BE_AUDIENCE': JSON.stringify(''),
    'process.env.DEV_AAD_CONFIG_BE_REDIRECT_URI': JSON.stringify(process.env.BACKEND_URL ?? ''),
    'process.env.WORKLOAD_NAME': JSON.stringify(process.env.WORKLOAD_NAME ?? ''),
    'process.env.DEFAULT_ITEM_NAME': JSON.stringify(process.env.DEFAULT_ITEM_NAME ?? ''),
    'process.env.DEV_WORKSPACE_ID': JSON.stringify(process.env.DEV_WORKSPACE_ID ?? ''),
    NODE_ENV: JSON.stringify(process.env.NODE_ENV ?? 'development'),
  },
});
