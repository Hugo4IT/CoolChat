@echo off

chcp 65001>NUL

echo|set /p="Restoring backend dependencies... "
dotnet restore
echo "Restored."

echo|set /p="Checking for User-Secrets... "
(dotnet user-secrets list --project CoolChat.Server.ASPNET && echo "Found.") || echo|set /p="Not found. Initializing... " && dotnet user-secrets init --project CoolChat.Server.ASPNET && dotnet user-secrets set "SecretSigningKey" "fcnh8u4h98qehn98h" --project CoolChat.Server.ASPNET && echo "Done."

echo|set /p="Checking for dotnet-ef... "
(dotnet tool install --global dotnet-ef && echo "Installed.") || dotnet tool update --global dotnet-ef && echo "Found."

echo|set /p="Applying migrations... "
dotnet ef database update --project CoolChat.Server.ASPNET
echo "Applied."

echo|set /p="Starting backend... "
start "CoolChat-Backend" dotnet run --project ./CoolChat.Server.ASPNET --launch-profile http
echo "Running."

echo|set /p="Restoring frontend dependencies... "
cd CoolChat.Client.Solid && npm i && cd ..
echo "Restored."

echo|set /p="Starting frontend... "
start "CoolChat-Frontend" (cd CoolChat.Client.Solid && npm run dev & cd ..)
echo "Running."

echo|set /p="Press <enter> to stop the servers... "
pause
echo

echo|set /p="Stopping frontend... "
taskkill /FI "WindowTitle eq CoolChat-Frontend*" /T /F
echo "Stopped."

echo|set /p="Stopping backend... "
taskkill /FI "WindowTitle eq CoolChat-Backend*" /T /F
echo "Stopped."
