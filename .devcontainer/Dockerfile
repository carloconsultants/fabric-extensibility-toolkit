FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0

# Install essential tools first and replace /bin/sh with bash
RUN apt-get update && apt-get install -y \
    bash \
    curl \
    gnupg \
    ca-certificates \
    wget \
    sudo \
    && rm -rf /var/lib/apt/lists/* \
    && rm /bin/sh \
    && ln -s /bin/bash /bin/sh

# Note: Removed mono-complete due to ARM64 compatibility issues
# Modern NuGet and .NET tools work without mono
# Node.js, npm, Azure Functions Core Tools, and Azure Static Web Apps CLI
# are installed via devcontainer features and postCreateCommand

# Set up user for development
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

USER $USERNAME