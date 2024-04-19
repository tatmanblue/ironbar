SET ROOT_PATH=%CD%
SET IRONBAR_BOOT_SERVER=http://localhost:50051
SET IRONBAR_DATA_PATH=f:\temp\ironbar
SET IRONBAR_RPC_PORT=50051
SET IRONBAR_TYPE=boot
ECHO Starting boot node....see output for configuration 
dotnet run --project ../src/node/ 
