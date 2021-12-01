FROM mcr.microsoft.com/dotnet/sdk:5.0-focal
RUN dotnet tool install dotnet-script --tool-path /usr/bin
COPY . /code
WORKDIR /code
ENTRYPOINT [ "dotnet", "script" ]

