import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import path from 'path';
import express from 'express';
import dotenv from 'dotenv';

// Load env from .env.* when running via `env-cmd -f .env.dev vite` or similar.
dotenv.config();

// Keep the same port/host Fabric DevGateway expects
const DEV_HOST = '127.0.0.1';
const DEV_PORT = 60006;

// Root of the app source; keep index.html in app/ to avoid moving files
const appRoot = path.resolve(__dirname, 'app');
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
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      configureServer(server: any) {
        // Mirror the headers/CORS behavior from webpack-dev-server setup
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        server.middlewares.use((req: any, res: any, next: any) => {
          res.setHeader('Access-Control-Allow-Origin', '*');
          res.setHeader('Access-Control-Allow-Methods', 'GET, PUT, POST, DELETE, OPTIONS');
          res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, Content-Length, X-Requested-With');
          if (req.method === 'OPTIONS') {
            res.statusCode = 204;
            return res.end();
          }
          next();
        });

        try {
          // Mount existing APIs implemented for the webpack dev server
          // devServer/index.js exports registerDevServerApis(app)
          // We wrap them in an Express app and plug that into Vite's connect middleware.
          // eslint-disable-next-line @typescript-eslint/no-var-requires
          const { registerDevServerApis } = require('./devServer');
          const app = express();
          app.use(express.json());
          registerDevServerApis(app);
          server.middlewares.use(app);

          // Helpful logs similar to the previous setup
          // eslint-disable-next-line no-console
          console.log('*********************************************************************');
          // eslint-disable-next-line no-console
          console.log('****               Vite server listening on port 60006           ****');
          // eslint-disable-next-line no-console
          console.log('****   You can now override the Fabric manifest with your own.   ****');
          // eslint-disable-next-line no-console
          console.log('*********************************************************************');
        } catch (e) {
          // eslint-disable-next-line no-console
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
    // Preserve file extensions from the original project
    extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json'],
  },
  define: {
  // Align with webpack.config.js mapping to ensure manifest endpoints get values
  // These mimic the webpack config that sets DEV_AAD_CONFIG_* from FRONTEND/BACKEND_* envs
  // Also expose these to the browser code where process.env.* is used
  'process.env.DEV_AAD_CONFIG_FE_APPID': JSON.stringify(process.env.FRONTEND_APPID ?? ''),
  'process.env.DEV_AAD_CONFIG_BE_APPID': JSON.stringify(process.env.BACKEND_APPID ?? ''),
  'process.env.DEV_AAD_CONFIG_BE_AUDIENCE': JSON.stringify(''),
  'process.env.DEV_AAD_CONFIG_BE_REDIRECT_URI': JSON.stringify(process.env.BACKEND_URL ?? ''),
    // Preserve existing process.env usage without renaming to VITE_*
    'process.env.WORKLOAD_NAME': JSON.stringify(process.env.WORKLOAD_NAME ?? ''),
    'process.env.DEFAULT_ITEM_NAME': JSON.stringify(process.env.DEFAULT_ITEM_NAME ?? ''),
    'process.env.DEV_WORKSPACE_ID': JSON.stringify(process.env.DEV_WORKSPACE_ID ?? ''),
    NODE_ENV: JSON.stringify(process.env.NODE_ENV ?? 'development'),
  },
});
