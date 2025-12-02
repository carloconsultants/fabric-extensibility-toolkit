#!/bin/bash

# Rebuild dev container without cache
# This ensures Docker doesn't use old cached layers

echo "🧹 Cleaning up old Docker build cache..."
docker builder prune -f

echo "🔄 Rebuilding dev container without cache..."
echo "   This will take a few minutes but ensures a fresh build."

# Note: This script is informational only
# Use VS Code Command Palette: "Dev Containers: Rebuild Container Without Cache"
