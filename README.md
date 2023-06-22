dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --no-self-contained
