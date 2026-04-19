#!/usr/bin/env bash
# Simple secret scanner for *nix CI environments
# Usage: ./scripts/secret-scan.sh [path]
set -euo pipefail
PATH_TO_SCAN="${1:-.}"
echo "Secret scan starting in path: $PATH_TO_SCAN"
# Exclude common build dirs
EXCLUDES=(".git" "bin" "obj" "node_modules" ".vs")
# Patterns to search (grep -E)
PATTERNS=(
  'Jwt\\s*[:=]\\s*SigningKey' 
  'Jwt"\\s*:\\s*\\{' 
  'ConnectionStrings"' 
  'Server=.*;.*Database=.*;' 
  '[Pp]assword[[:space:]]*=' 
  '[Uu]ser[[:space:]]Id[[:space:]]*=' 
  '\\.local\\.json$'
)
# Build find exclude args
FIND_EXCLUDE_ARGS=()
for e in "${EXCLUDES[@]}"; do
  FIND_EXCLUDE_ARGS+=( -path "./$e" -prune -o )
done
# Construct grep pattern
GREP_PATTERN=$(IFS='|'; echo "${PATTERNS[*]}")
# Run find and grep
FOUND=0
while IFS= read -r -d $'\0' file; do
  if grep -En -H -m1 -e "$GREP_PATTERN" "$file" >/dev/null; then
    echo "Potential secret in: $file"
    grep -En -H -m1 -e "$GREP_PATTERN" "$file"
    FOUND=1
  fi
done < <(find "$PATH_TO_SCAN" "${FIND_EXCLUDE_ARGS[@]}" -type f -print0)

if [ "$FOUND" -eq 1 ]; then
  echo "Potential secrets found. Failing CI step." >&2
  exit 1
else
  echo "No likely secrets found."
  exit 0
fi
