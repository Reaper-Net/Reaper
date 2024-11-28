FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       clang zlib1g-dev
COPY benchmarks/BenchmarkWeb .
RUN dotnet publish "BenchmarkWeb.csproj" -c Release -o /app/publish /p:DefineConstants=MINIMAL /p:PublishAot=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./BenchmarkWeb"]
