# Development Container Configuration for PBI.tips Workload Template

This directory contains the configuration for GitHub Codespaces and Visual Studio Code Remote Containers. It provides a consistent development environment for building Microsoft Fabric workloads with all the necessary tools pre-installed.

## What's Included

- **.NET SDK 8.0**: For building .NET Azure Functions
- **PowerShell**: For running setup scripts and automation
- **Node.js LTS**: For Vite frontend development
- **Azure CLI**: For Azure resource management
- **Azure Functions Core Tools v4**: For local Functions development
- **Azure Static Web Apps CLI**: For full-stack local development
- **Common utilities**: Git, curl, jq, etc.

## Port Configuration

The following ports are forwarded from the container to your local machine:

- **3000**: For general frontend development
- **60006**: Microsoft Fabric frontend gateway
- **7071**: Azure Functions local runtime

## Getting Started

1. Open this repository in GitHub Codespaces or using VS Code Remote Containers
2. Wait for the container to build and initialize
3. Run the setup script to configure your development environment:

   ```bash
   ./.devcontainer/setup-dev-environment.sh
   ```

4. Follow the instructions in the main [README.md](../README.md) to start developing

## Development Workflow

```bash
# Start full-stack development
swa start --config fabric-workload

# Or start services individually
npm run start              # Frontend (Vite) 
func start                 # Backend (Functions) - from api/ directory
```

## Customizing the Configuration

If you need to make changes to the development container:

1. Modify the `devcontainer.json` file to add extensions or change settings
2. Update the `Dockerfile` to install additional dependencies
3. Rebuild the container to apply your changes

This template is optimized for PBI.tips workload development patterns and Microsoft Fabric extensibility.