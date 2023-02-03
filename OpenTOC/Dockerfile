FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OpenTOC/OpenTOC.csproj", "OpenTOC/"]
RUN dotnet restore "OpenTOC/OpenTOC.csproj"
COPY . .
WORKDIR "/src/OpenTOC"
RUN dotnet build "OpenTOC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenTOC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenTOC.dll"]
