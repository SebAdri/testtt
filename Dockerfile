FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CleanArchitecture.Full.Domain/*.csproj src/CleanArchitecture.Full.Domain/
COPY src/CleanArchitecture.Full.Application/*.csproj src/CleanArchitecture.Full.Application/
COPY src/CleanArchitecture.Full.Infrastructure/*.csproj src/CleanArchitecture.Full.Infrastructure/
COPY src/CleanArchitecture.Full.Api/*.csproj src/CleanArchitecture.Full.Api/
RUN dotnet restore src/CleanArchitecture.Full.Api/CleanArchitecture.Full.Api.csproj

COPY src/ src/
RUN dotnet publish src/CleanArchitecture.Full.Api/CleanArchitecture.Full.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CleanArchitecture.Full.Api.dll"]
