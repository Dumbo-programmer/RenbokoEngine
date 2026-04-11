#!/usr/bin/env bash
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "Usage: ./scripts/tag-release.sh v0.2.0"
  exit 1
fi

VERSION="$1"

if [ -n "$(git status --porcelain)" ]; then
  echo "Working tree is not clean. Commit or stash changes before tagging."
  exit 1
fi

if git rev-parse "$VERSION" >/dev/null 2>&1; then
  echo "Tag '$VERSION' already exists."
  exit 1
fi

git tag -a "$VERSION" -m "Release $VERSION"
echo "Created tag: $VERSION"
echo "Push with: git push origin $VERSION"
