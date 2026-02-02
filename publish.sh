#!/bin/bash
set -e

# ============================================================================
# publish.sh - Build and package idas-cli for Windows and Linux
# 
# Usage:
#   ./publish.sh [version]
#
# If version is not provided, defaults to "0.0.1"
# Can be run locally (WSL/Git Bash) or from GitHub Actions
# ============================================================================

VERSION="${1:-0.0.1}"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="${PROJECT_DIR}/dist"
PUBLISH_BASE="${PROJECT_DIR}/bin/publish"

echo "============================================"
echo "Building idas-cli version ${VERSION}"
echo "============================================"

# Clean previous build artifacts
rm -rf "${DIST_DIR}"
rm -rf "${PUBLISH_BASE}"
mkdir -p "${DIST_DIR}"

# Define platforms to build
PLATFORMS=("win-x64" "linux-x64")

for PLATFORM in "${PLATFORMS[@]}"; do
    echo ""
    echo "--------------------------------------------"
    echo "Building for ${PLATFORM}..."
    echo "--------------------------------------------"
    
    PUBLISH_DIR="${PUBLISH_BASE}/${PLATFORM}"
    
    # Publish as self-contained single file
    # Note: We skip separate restore/build and let publish handle everything
    # to ensure the correct self-contained binaries are produced
    # PublishTrimmed is disabled due to reflection usage in the codebase
    echo "Publishing self-contained single-file executable..."
    dotnet publish "${PROJECT_DIR}/idas.csproj" \
        -c Release \
        -r "${PLATFORM}" \
        -p:PublishSingleFile=true \
        -p:SelfContained=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="${VERSION}" \
        -o "${PUBLISH_DIR}"
    
    # Copy README to publish directory
    cp "${PROJECT_DIR}/README.md" "${PUBLISH_DIR}/"
    
    # Package based on platform
    echo "Packaging..."
    ARCHIVE_NAME="idas-${VERSION}-${PLATFORM}"
    
    if [[ "${PLATFORM}" == win-* ]]; then
        # Create zip for Windows
        (cd "${PUBLISH_DIR}" && zip -r "${DIST_DIR}/${ARCHIVE_NAME}.zip" .)
        echo "Created: ${DIST_DIR}/${ARCHIVE_NAME}.zip"
    else
        # Create tar.gz for Linux/macOS
        (cd "${PUBLISH_DIR}" && tar -czvf "${DIST_DIR}/${ARCHIVE_NAME}.tar.gz" .)
        echo "Created: ${DIST_DIR}/${ARCHIVE_NAME}.tar.gz"
    fi
done

echo ""
echo "============================================"
echo "Build complete! Artifacts in ${DIST_DIR}:"
ls -la "${DIST_DIR}"
echo "============================================"
