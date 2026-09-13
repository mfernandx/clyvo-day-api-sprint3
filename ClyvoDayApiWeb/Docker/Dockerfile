# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copia apenas o csproj primeiro para aproveitar o cache do Docker
COPY ClyvoDayApiWeb/ClyvoDayApiWeb.csproj ClyvoDayApiWeb/

WORKDIR /src/ClyvoDayApiWeb

RUN dotnet restore

# Copia o restante dos arquivos da API
COPY ClyvoDayApiWeb/. .

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish --no-restore


# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copia apenas os arquivos publicados
COPY --from=build /app/publish .

# Porta HTTP da aplicação
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Executa como usuário não privilegiado
USER app

ENTRYPOINT ["dotnet", "ClyvoDayApiWeb.dll"]