# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Nyxon.Core/Nyxon.Core.csproj", "Nyxon.Core/"]
COPY ["Nyxon.Server/Nyxon.Server.csproj", "Nyxon.Server/"]
RUN dotnet restore "Nyxon.Server/Nyxon.Server.csproj"

COPY . .
WORKDIR "/src/Nyxon.Server"
RUN dotnet publish -c Release -o /Nyxon/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /Nyxon
# (In your runtime/final stage)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /Nyxon/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nyxon.Server.dll"]