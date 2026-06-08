FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ConnectionStrings__SQLiteConnection="Data Source=/tmp/equipment.db"
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TaktTusur.Equipment.Api/TaktTusur.Equipment.Api.csproj", "TaktTusur.Equipment.Api/"]
RUN dotnet restore "TaktTusur.Equipment.Api/TaktTusur.Equipment.Api.csproj"
COPY . .
WORKDIR "/src/TaktTusur.Equipment.Api"
RUN dotnet build "TaktTusur.Equipment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaktTusur.Equipment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaktTusur.Equipment.Api.dll"]
