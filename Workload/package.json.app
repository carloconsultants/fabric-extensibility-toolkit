{
  "name": "pbitips-workload-template",
  "version": "1.0.0",
  "description": "PBI.tips Workload Template - Microsoft Fabric workload development template",
  "scripts": {
    "start": "npm run start:vite",
    "start:vite": "env-cmd -f .env.dev vite --config ./vite.config.mts",
    "start:devGateway": "node ./devServer/start-devGateway.js",
    "build:test": "env-cmd -f .env.test vite build --config ./vite.config.mts",
    "build:prod": "env-cmd -f .env.prod vite build --config ./vite.config.mts"
  },
  "keywords": [
    "microsoft-fabric",
    "workload",
    "powerbi",
    "pbitips",
    "template",
    "vite",
    "react",
    "typescript"
  ],
  "author": "PBI.tips",
  "license": "MIT",
  "dependencies": {
    "@carloconsultants/cs-ui-library": "^0.0.67",
    "@fluentui/react": "^8.110.7",
    "@fluentui/react-components": "^9.7.2",
    "@fluentui/react-icons": "^2.0.297",
    "@ms-fabric/workload-client": "^3.0.0",
    "@reduxjs/toolkit": "^2.6.0",
    
    "history": "^4.9.0",
    "i18next": "^25.4.1",
    "i18next-http-backend": "^2.4.2",
    "jwt-decode": "^3.1.2",
    "react": "^18.0.0",
    "react-dom": "^18.0.0",
    "react-i18next": "^15.0.1",
    "react-redux": "^9.2.0",
    "react-router-dom": "^5.3.4",
    "uuid": "^11.0.5"
  },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "@types/react": "^18.0.0",
    "@types/react-dom": "^18.0.0",
    "@types/react-router-dom": "^5.3.2",
    "@vitejs/plugin-react": "^4.7.0",
    "ajv": "^8.12.0",
    "cors": "^2.8.5",
    "dotenv": "^16.4.5",
    "env-cmd": "^10.1.0",
    "express": "^5.1.0",
    "nuget-bin": "^4.0.0",
    "sass": "^1.79.3",
    "typescript": "^5.0.4",
    "vite": "^7.1.7",
    "vite-plugin-env-compatible": "^2.0.1",
    "vite-plugin-static-copy": "^3.1.3",
    "vite-tsconfig-paths": "^5.1.3"
  }
}
