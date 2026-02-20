FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Install timezone data and set timezone
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*
ENV TZ=America/Sao_Paulo
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY ["SW_PortalCliente_BeachPark.sln", "."]
COPY ["SW_PortalCliente_BeachPark.API.csproj", "."]
COPY ["SW_PortalProprietario.Infra.Ioc/SW_PortalProprietario.Infra.Ioc.csproj", "SW_PortalProprietario.Infra.Ioc/"]
COPY ["SW_PortalProprietario.Application/SW_PortalProprietario.Application.csproj", "SW_PortalProprietario.Application/"]
COPY ["SW_PortalProprietario.Domain/SW_PortalProprietario.Domain.csproj", "SW_PortalProprietario.Domain/"]
COPY ["SW_PortalProprietario.Infra.Data/SW_PortalProprietario.Infra.Data.csproj", "SW_PortalProprietario.Infra.Data/"]
COPY ["SW_Utils/SW_Utils.csproj", "SW_Utils/"]
COPY ["AccessCenterDomain/AccessCenterDomain.csproj", "AccessCenterDomain/"]
COPY ["EsolutionPortalDomain/EsolutionPortalDomain.csproj", "EsolutionPortalDomain/"]
COPY ["CMDomain/CMDomain.csproj", "CMDomain/"]

# Restore dependencies
RUN dotnet restore "SW_PortalCliente_BeachPark.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
RUN dotnet publish "SW_PortalCliente_BeachPark.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SW_PortalCliente_BeachPark.API.dll"]
