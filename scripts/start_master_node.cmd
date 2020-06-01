SET ROOT_PATH=%CD%
MKDIR data
ECHO "%ROOT_PATH%\data"
dotnet run --project ../src/node/ -- "%ROOT_PATH%\boot_node.config"
