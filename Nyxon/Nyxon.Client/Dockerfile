# 1. Build the Blazor WASM app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "Nyxon.Client/Nyxon.Client.csproj" -c Release -o /app/publish

# 2. Serve it with Caddy (or Nginx)
FROM caddy:alpine
COPY --from=build /app/publish/wwwroot /usr/share/caddy
# We will use the production Caddyfile to route traffic