#!/usr/bin/env bash
ROOT_PATH="$(cd "$(dirname "$1")"; pwd)/$(basename "$1")"
mkdir -p data
echo "${ROOT_PATH}data"
dotnet run --project ../src/node/ -- -d "${ROOT_PATH}data"
