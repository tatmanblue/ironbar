SET ROOT_PATH=%CD%
SET PORT=5002
ECHO "%ROOT_PATH%\data"

dotnet run --project ../src/node/ -- -d "%ROOT_PATH%\data"  -r %PORT%
