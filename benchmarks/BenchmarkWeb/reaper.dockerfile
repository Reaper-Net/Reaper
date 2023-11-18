FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "BenchmarkWeb.csproj" -c Release -o /app/publish /p:DefineConstants=REAPER /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BenchmarkWeb.dll"]
