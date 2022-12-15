FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore

WORKDIR /app/ProtocApi
RUN dotnet publish -c release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

RUN apt-get update \
 && DEBIAN_FRONTEND=noninteractive \
    apt-get install --no-install-recommends --assume-yes \
      protobuf-compiler

RUN apt install binutils git gnupg2 libc6-dev libcurl4  \
    libedit2 libgcc-9-dev libpython2.7 libsqlite3-0 libstdc++-9-dev  \
    libxml2 libz3-dev pkg-config tzdata zlib1g-dev

ENV PATH "$PATH:/app/protoc/linux64"
WORKDIR /app
COPY --from=build /out ./
RUN chmod -R +x /app/protoc/linux64
ENTRYPOINT ["dotnet", "ProtocApi.dll"]