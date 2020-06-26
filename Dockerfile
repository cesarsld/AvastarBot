FROM mcr.microsoft.com/dotnet/core/runtime:3.1

COPY /AvastarBot/bin/Release/netcoreapp3.1/publish/ app/
COPY /AvastarBot/create-traits-nosvg.json app/

ENTRYPOINT dotnet app/AvastarBot.dll "$TOKEN" "$MONGO_URL" "Release"