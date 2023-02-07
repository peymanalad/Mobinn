  FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
  RUN apt-get update && apt-get install -y libgdiplus
  RUN apt-get update && apt-get install openssh-server sudo nano -y
  RUN useradd -rm -d /home/deed -s /bin/bash -g root -G sudo -u 1000 deed
  RUN  echo 'deed:ch@mr@nch@mr@n' | chpasswd
  RUN service ssh start
  EXPOSE 22
  CMD ["/usr/sbin/sshd","-D"]
  COPY bin/Release/net6.0/publish/ App/
  WORKDIR /App
  EXPOSE 80
  EXPOSE 443
  ENTRYPOINT ["dotnet", "YasnaSystem.Deed.Web.Host.dll"]