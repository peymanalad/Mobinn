FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# Install necessary packages for SkiaSharp
RUN apt-get update && \
    apt-get install -y libgdiplus libfontconfig1 libc6-dev libgl1-mesa-glx libegl1-mesa libgles2-mesa-dev libgbm-dev mesa-utils

# Install nano, sudo, and openssh-server if needed
RUN apt-get update && \
    apt-get install -y sudo nano
    #openssh-server 

# Create a new user (if needed)
#RUN useradd -rm -d /home/deed -s /bin/bash -g root -G sudo -u 1000 deed
#RUN echo 'deed:ch@mr@nch@mr@n' | chpasswd

# Start SSH service (if needed)
#RUN service ssh start
#EXPOSE 22
#CMD ["/usr/sbin/sshd", "-D"]

# Copy application to container
COPY bin/Release/net7.0/publish/ App/
WORKDIR /App

# Expose ports
EXPOSE 80
EXPOSE 443

# Start application
ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]