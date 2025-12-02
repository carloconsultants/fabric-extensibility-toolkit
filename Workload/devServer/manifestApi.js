/**
 * Manifest API implementation
 * Handles serving of manifest metadata and package files
 * Note: Uses native Node.js HTTP methods (not Express) for Vite/Connect compatibility
 */

const fs = require('fs').promises;
const fsSync = require('fs');
const path = require('path');
const { buildManifestPackage } = require('./build-manifest');

/**
 * Middleware function to handle manifest API routes
 * Compatible with Connect middleware (used by Vite)
 */
function manifestApi(req, res, next) {
  const url = req.url;
  
  // Handle OPTIONS requests (CORS preflight)
  if (req.method === 'OPTIONS') {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
    res.setHeader('Access-Control-Max-Age', '86400');
    res.statusCode = 204;
    res.end();
    console.log("Handled CORS preflight request for manifest endpoint.");
    return;
  }

  // Handle GET /manifests_new/metadata
  if (req.method === 'GET' && url === '/manifests_new/metadata') {
    res.writeHead(200, {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization'
    });

    const devParameters = {
      name: process.env.WORKLOAD_NAME,
      url: "http://127.0.0.1:60006",
      devAADFEAppConfig: {
        appId: process.env.DEV_AAD_CONFIG_FE_APPID,
      }
    };

    res.end(JSON.stringify({ extension: devParameters }));
    console.log("Delivered manifest metainformation successfully.");
    return;
  }

  // Handle GET /manifests_new
  if (req.method === 'GET' && url === '/manifests_new') {
    (async () => {
      try {
        await buildManifestPackage(); // Wait for the build to complete
        const filePath = path.resolve(__dirname, `../../build/Manifest/${process.env.WORKLOAD_NAME}.${process.env.WORKLOAD_VERSION}.nupkg`);
        
        // Check if the file exists
        await fs.access(filePath);
        const stats = await fs.stat(filePath);

        // Set headers before sending file
        res.setHeader('Content-Type', 'application/octet-stream');
        res.setHeader('Content-Disposition', `attachment; filename="ManifestPackage.1.0.0.nupkg"`);
        res.setHeader('Content-Length', stats.size);
        res.setHeader('Access-Control-Allow-Origin', '*');
        res.setHeader('Access-Control-Allow-Methods', 'GET');
        res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');
        res.statusCode = 200;

        // Stream the file using native Node.js
        const fileStream = fsSync.createReadStream(filePath);
        fileStream.pipe(res);
        fileStream.on('end', () => {
          console.log("Delivered manifest package successfully.");
        });
        fileStream.on('error', (err) => {
          console.error(`❌ Error streaming file: ${err.message}`);
          if (!res.headersSent) {
            res.statusCode = 500;
            res.setHeader('Content-Type', 'application/json');
            res.end(JSON.stringify({
              error: "Failed to stream manifest package",
              details: err.message
            }));
          }
        });
      } catch (err) {
        console.error(`❌ Error: ${err.message}`);
        res.statusCode = 500;
        res.setHeader('Content-Type', 'application/json');
        res.end(JSON.stringify({
          error: "Failed to serve manifest package",
          details: err.message
        }));
      }
    })();
    return;
  }

  // Not a manifest API route, pass to next middleware
  next();
}

module.exports = manifestApi;