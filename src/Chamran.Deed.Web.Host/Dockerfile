FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# نصب پیش‌نیازهای SkiaSharp و ffmpeg و ابزارهای لازم
RUN apt-get update && \
    apt-get install -y \
        libgdiplus libfontconfig1 libc6-dev libgl1-mesa-glx libegl1-mesa \
        libgles2-mesa-dev libgbm-dev mesa-utils \
        ffmpeg sudo nano tzdata && \
    rm -rf /var/lib/apt/lists/*

# تنظیمات SkiaSharp
ENV LD_LIBRARY_PATH=/usr/local/lib
RUN echo "/usr/local/lib" > /etc/ld.so.conf.d/usr-local-lib.conf && ldconfig

# تنظیم تایم‌زون به تهران
ENV TZ=Asia/Tehran

# کپی اپلیکیشن
COPY bin/Release/net7.0/publish/ App/
WORKDIR /App

# باز کردن پورت‌ها
EXPOSE 80
EXPOSE 443

# اجرای اپلیکیشن
ENTRYPOINT ["dotnet", "Chamran.Deed.Web.Host.dll"]
