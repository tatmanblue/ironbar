#!/usr/bin/env bash
ROOT_PATH="$(cd "$(dirname "$1")"; pwd)/$(basename "$1")"
export IRONBAR_BOOT_SERVER=http://localhost:50051
export IRONBAR_DATA_PATH=f:\temp\ironbar
export IRONBAR_RPC_PORT=50051
export IRONBAR_TYPE=boot
ECHO Starting boot node....see output for configuration 
dotnet run --project ../src/node/

