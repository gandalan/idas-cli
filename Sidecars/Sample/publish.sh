#!/bin/bash
set -e

VERSION="${1:-0.1.0}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
DIST_DIR="${ROOT_DIR}/dist"
PUBLISH_BASE="${SCRIPT_DIR}/bin/publish"
PROJECT_FILE="${SCRIPT_DIR}/IdasSidecarSample.csproj"
PLATFORMS=("win-x64" "linux-x64")

mkdir -p "${DIST_DIR}"
rm -rf "${PUBLISH_BASE}"

echo "============================================"
echo "Building sidecar sample version ${VERSION}"
echo "============================================"

for PLATFORM in "${PLATFORMS[@]}"; do
    echo ""
    echo "--------------------------------------------"
    echo "Building sidecar for ${PLATFORM}..."
    echo "--------------------------------------------"

    PUBLISH_DIR="${PUBLISH_BASE}/${PLATFORM}"

    dotnet publish "${PROJECT_FILE}" \
        -c Release \
        -r "${PLATFORM}" \
        -p:PublishSingleFile=true \
        -p:SelfContained=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="${VERSION}" \
        -o "${PUBLISH_DIR}"

    if [[ "${PLATFORM}" == win-* ]]; then
        cp "${PUBLISH_DIR}/idas-beispiel.exe" "${DIST_DIR}/idas-beispiel.exe"
    else
        cp "${PUBLISH_DIR}/idas-beispiel" "${DIST_DIR}/idas-beispiel"
    fi
done

echo ""
echo "============================================"
echo "Sidecar sample copied to ${DIST_DIR}:"
ls -la "${DIST_DIR}" | grep 'idas-beispiel' || true
echo "============================================"
