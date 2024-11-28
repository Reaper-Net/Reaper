FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet pack src/Reaper.SourceGenerator/Reaper.SourceGenerator.csproj -c Release -o /app/nuget
RUN mv /app/nuget/*.nupkg /app/nuget/Reaper.SourceGenerator.1.0.0.nupkg
RUN dotnet nuget add source /app/nuget
RUN dotnet publish "benchmarks/BenchmarkWeb/BenchmarkWeb.csproj" -c Release -o /app/publish /p:DefineConstants=REAPER /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BenchmarkWeb.dll"]
