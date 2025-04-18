FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# Clear the package cache
RUN rm -rf /var/lib/apt/lists/*
RUN apt-get clean

# Install necessary packages for SkiaSharp
RUN apt-get update && \
    apt-get install -y libgdiplus libfontconfig1 libc6-dev libgl1-mesa-glx libegl1-mesa libgles2-mesa-dev libgbm-dev mesa-utils

# Add environment variable for SkiaSharp
ENV LD_LIBRARY_PATH=/usr/local/lib

# Run ldconfig to refresh the linker cache
RUN echo "/usr/local/lib" > /etc/ld.so.conf.d/usr-local-lib.conf && ldconfig

# Install nano, sudo, and openssh-server if needed
RUN apt-get -o Acquire::Check-Valid-Until=false update && \
    apt-get install -y sudo nano

# Install tzdata package
RUN apt-get install -y tzdata

# Set the time zone
ENV TZ=Asia/Tehran

# Copy application to container
COPY bin/Release/net7.0/publish/ App/
WORKDIR /App

# Expose ports
EXPOSE 80
EXPOSE 443

# Start application
ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]
