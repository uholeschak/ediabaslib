# Copilot Instructions

## General Guidelines
- After every file edit, verify the result by re-reading the edited lines (e.g., URLs/comments containing '//' can get truncated by the edit tool) and ensure the file encoding (UTF-8 BOM) and line endings are preserved.

## Project-Specific Rules
- In the ApkUploader project, the Google Play "beta" track is configured as an open testing track; UserFraction (staged rollout) is not allowed there, only on production and closed testing tracks (e.g. alpha).