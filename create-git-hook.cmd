@echo off
if not exist .git (
    echo "Run this command at the root of your GIT repository"
    exit -1
)

(echo #!/bin/sh & echo "/c/Program Files/nodejs/node.exe" "$APPDATA/npm/node_modules/@commitlint/cli/lib/cli.js" -e $1) > %~dp0\.git\hooks\commit-msg
npm i -g @commitlint/cli @commitlint/config-angular
