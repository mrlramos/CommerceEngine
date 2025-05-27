# Usar a imagem base do .NET 8 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar o arquivo de projeto e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar todo o código fonte e fazer build
COPY . ./
RUN dotnet publish -c Release -o out

# Usar a imagem runtime do .NET 8 para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar os arquivos publicados
COPY --from=build /app/out .

# Expor a porta
EXPOSE 80

# Definir o ponto de entrada
ENTRYPOINT ["dotnet", "Order.API.dll"] 