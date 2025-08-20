#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM microsoft.repo.stinascloud.ir/dotnet/aspnet:7.0 AS base
WORKDIR /App
EXPOSE 80
EXPOSE 443

RUN apt-get update && \
    apt-get install -y \
        libgdiplus libfontconfig1 libc6-dev libgl1-mesa-glx libegl1-mesa \
        libgles2-mesa-dev libgbm-dev mesa-utils \
        ffmpeg imagemagick sudo vim nano tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV LD_LIBRARY_PATH=/usr/local/lib
RUN echo "/usr/local/lib" > /etc/ld.so.conf.d/usr-local-lib.conf && ldconfig

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
RUN dotnet build "Chamran.Deed.Web.Host.csproj" -c Release -o /App/build

FROM build AS publish
RUN dotnet publish "Chamran.Deed.Web.Host.csproj" -c Release -o /App/publish

FROM base AS final
WORKDIR /App
COPY --from=publish /App/publish .
ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]
