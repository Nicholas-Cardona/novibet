FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0 AS build
WORKDIR /src

COPY . .
WORKDIR /src/Novibet.Worker

RUN dotnet restore 
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.8-azurelinux3.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Novibet.Worker.dll"]