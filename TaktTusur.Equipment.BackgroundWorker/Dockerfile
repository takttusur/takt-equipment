FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TaktTusur.Equipment.BackgroundWorker/TaktTusur.Equipment.BackgroundWorker.csproj", "TaktTusur.Equipment.BackgroundWorker/"]
RUN dotnet restore "TaktTusur.Equipment.BackgroundWorker/TaktTusur.Equipment.BackgroundWorker.csproj"
COPY . .
WORKDIR "/src/TaktTusur.Equipment.BackgroundWorker"
RUN dotnet build "TaktTusur.Equipment.BackgroundWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaktTusur.Equipment.BackgroundWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaktTusur.Equipment.BackgroundWorker.dll"]
