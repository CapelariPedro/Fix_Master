
#!/usr/bin/env bash

set -Eeuo pipefail

# FIXMASTER - BUILD


ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

PROJECT="$ROOT_DIR/FixMaster/FixMaster.csproj"
RELEASE_DIR="$ROOT_DIR/releases"

CONFIGURATION="Release"

echo ""
echo "FIXMASTER BUILD SYSTEM"
echo ""

# Verificar .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "ERRO: .NET SDK não encontrado."
    exit 1
fi

# Restaurar dependências
echo "[1/5] Restaurando dependências..."
dotnet restore "$PROJECT"

# Função de publicação
publish() {
    local RID="$1"
    local OUTPUT="$2"

    echo ""
    echo "Publicando: $RID"

    dotnet publish "$PROJECT" \
        -c "$CONFIGURATION" \
        -r "$RID" \
        --self-contained true \
        -o "$RELEASE_DIR/$OUTPUT"
}

# Linux Fedora x64
echo "[2/5] Linux Fedora x64..."
publish "linux-x64" "linux-x64"

chmod +x "$RELEASE_DIR/linux-x64/FixMaster"

# Windows 11 x64
echo "[3/5] Windows 11 x64..."
publish "win-x64" "win-x64"

# macOS Intel
echo "[4/5] macOS Intel..."
publish "osx-x64" "macos-x64"

# macOS Apple Silicon
echo "[5/5] macOS Apple Silicon..."
publish "osx-arm64" "macos-arm64"

echo ""
echo "BUILD CONCLUIDO!"
echo ""

echo "Linux:          releases/linux-x64/"
echo "Windows:        releases/win-x64/"
echo "macOS Intel:    releases/macos-x64/"
echo "macOS ARM64:    releases/macos-arm64/"
