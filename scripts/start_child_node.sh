export IRONBAR_BOOT_SERVER=http://localhost:50051
export IRONBAR_DATA_PATH=f:\temp\ironbar
export IRONBAR_RPC_PORT=50055
export IRONBAR_TYPE=child
ECHO Starting child node....see output for configuration 
dotnet run --project ../src/node/
