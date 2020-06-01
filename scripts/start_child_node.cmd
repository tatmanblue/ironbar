SET ROOT_PATH=%CD%
SET PORT=5002
ECHO "%ROOT_PATH%\data"

dotnet run --project ../src/node/ -- "%ROOT_PATH%\child_node_1.config"
