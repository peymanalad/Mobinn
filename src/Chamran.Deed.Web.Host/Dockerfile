  FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
  RUN apt-get update && apt-get install -y libgdiplus
  RUN apt-get update && apt-get install sudo nano -y 
  #openssh-server 
  #RUN useradd -rm -d /home/deed -s /bin/bash -g root -G sudo -u 1000 deed
  #RUN  echo 'deed:ch@mr@nch@mr@n' | chpasswd
  #RUN service ssh start
  #EXPOSE 22
  #CMD ["/usr/sbin/sshd","-D"]
  COPY bin/Release/net7.0/publish/ App/
  WORKDIR /App
  EXPOSE 80
  EXPOSE 443
  ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]