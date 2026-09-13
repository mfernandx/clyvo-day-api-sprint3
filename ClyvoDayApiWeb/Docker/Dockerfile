# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copia os arquivos .csproj necessários
COPY ClyvoDayApiWeb/ClyvoDayApiWeb.csproj ClyvoDayApiWeb/
COPY ClyvoDayApiWeb.Infrastructure/ClyvoDayApiWeb.Infrastructure.csproj ClyvoDayApiWeb.Infrastructure/

# Restaura as dependências da API
RUN dotnet restore ClyvoDayApiWeb/ClyvoDayApiWeb.csproj

# Copia os códigos dos projetos
COPY ClyvoDayApiWeb/. ClyvoDayApiWeb/
COPY ClyvoDayApiWeb.Infrastructure/. ClyvoDayApiWeb.Infrastructure/

# Entra na pasta da API
WORKDIR /src/ClyvoDayApiWeb

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish --no-restore


# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Porta HTTP da aplicação
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Executa como usuário não privilegiado
USER app

ENTRYPOINT ["dotnet", "ClyvoDayApiWeb.dll"]