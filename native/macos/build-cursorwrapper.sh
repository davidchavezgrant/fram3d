#!/bin/zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
SDK_PATH="$(xcrun --sdk macosx --show-sdk-path)"
OUT_DYLIB="$ROOT_DIR/Unity/Fram3d/Assets/NativeCursor/Scripts/Native/MacOS/Plugins/CursorWrapper.dylib"
SRC_FILE="$ROOT_DIR/native/macos/CursorWrapper.mm"

xcrun clang++ \
  -dynamiclib \
  -fobjc-arc \
  -framework Cocoa \
  -isysroot "$SDK_PATH" \
  -arch arm64 \
  -arch x86_64 \
  -o "$OUT_DYLIB" \
  "$SRC_FILE"
