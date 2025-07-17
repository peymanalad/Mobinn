#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft.repo.stinascloud.ir/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Chamran.Deed.Web.Host/Chamran.Deed.Web.Host.csproj", "src/Chamran.Deed.Web.Host/"]
COPY ["src/Chamran.Deed.Web.Core/Chamran.Deed.Web.Core.csproj", "src/Chamran.Deed.Web.Core/"]
COPY ["src/Chamran.Deed.Application/Chamran.Deed.Application.csproj", "src/Chamran.Deed.Application/"]
COPY ["src/Chamran.Deed.Application.Shared/Chamran.Deed.Application.Shared.csproj", "src/Chamran.Deed.Application.Shared/"]
COPY ["src/Chamran.Deed.Core.Shared/Chamran.Deed.Core.Shared.csproj", "src/Chamran.Deed.Core.Shared/"]
COPY ["src/Chamran.Deed.Core/Chamran.Deed.Core.csproj", "src/Chamran.Deed.Core/"]
COPY ["src/Chamran.Deed.EntityFrameworkCore/Chamran.Deed.EntityFrameworkCore.csproj", "src/Chamran.Deed.EntityFrameworkCore/"]
COPY ["src/Chamran.Deed.GraphQL/Chamran.Deed.GraphQL.csproj", "src/Chamran.Deed.GraphQL/"]
RUN dotnet restore "src/Chamran.Deed.Web.Host/Chamran.Deed.Web.Host.csproj" -s "https://nexus.repo.stinascloud.ir/repository/nuget/index.json"
COPY . .
WORKDIR "/src/src/Chamran.Deed.Web.Host"
RUN dotnet build "Chamran.Deed.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chamran.Deed.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]
