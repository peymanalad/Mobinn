#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM microsoft.repo.stinascloud.ir/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY common.props ./common.props
COPY ["src/Chamran.Deed.Migrator/Chamran.Deed.Migrator.csproj", "src/Chamran.Deed.Migrator/"]
COPY ["src/Chamran.Deed.EntityFrameworkCore/Chamran.Deed.EntityFrameworkCore.csproj", "src/Chamran.Deed.EntityFrameworkCore/"]
COPY ["src/Chamran.Deed.Core/Chamran.Deed.Core.csproj", "src/Chamran.Deed.Core/"]
COPY ["src/Chamran.Deed.Core.Shared/Chamran.Deed.Core.Shared.csproj", "src/Chamran.Deed.Core.Shared/"]
RUN dotnet restore "src/Chamran.Deed.Migrator/Chamran.Deed.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Chamran.Deed.Migrator"
RUN dotnet build "Chamran.Deed.Migrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chamran.Deed.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chamran.Deed.Migrator.dll", "-s"]
