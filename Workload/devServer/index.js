/**
 * DevServer APIs index file
 * Exports all API routers for the dev server
 */

const manifestApi = require('./manifestApi');
const schemaApi = require('./schemaApi')

/**
 * Register all dev server APIs with an Express application
 * @param {object} app Express application
 */
function registerDevServerApis(app) {
  console.log('*** Starting Dev Server API Registration ***');
  
  console.log('*** Mounting Manifest API ***');
  app.use('/', manifestApi);
  app.use('/', schemaApi);
  
  console.log('*** Dev Server API Registration Complete ***');
}

module.exports = {
  manifestApi,
  registerDevServerApis
};
