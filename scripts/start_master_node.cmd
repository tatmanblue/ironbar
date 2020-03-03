SET ROOT_PATH=%CD%
MKDIR data
ECHO "%ROOT_PATH%\data"
dotnet run --project ../src/node/ -- -d "%ROOT_PATH%\data"
