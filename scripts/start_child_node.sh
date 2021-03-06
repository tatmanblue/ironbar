#!/usr/bin/env bash
# start_child_node datapath port

ROOT_PATH="$(cd "$(dirname "$1")"; pwd)/$(basename "$1")"
shift
PORT=5002
mkdir -p data
echo "${ROOT_PATH}data"
dotnet run --project ../src/node/ -- "${ROOT_PATH}child_node_1.config"
