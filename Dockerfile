FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore

WORKDIR /app/ProtocApi
RUN dotnet publish -c release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# main protoc dependencies
RUN apt-get update \
 && DEBIAN_FRONTEND=noninteractive \
    apt-get install --no-install-recommends --assume-yes \
      protobuf-compiler

# swift
RUN apt-get -y install \
      build-essential       \
      clang                 \
      cmake                 \
      git                   \
      icu-devtools          \
      libcurl4-openssl-dev  \
      libedit-dev           \
      libicu-dev            \
      libncurses5-dev       \
      libpython3-dev        \
      libsqlite3-dev        \
      libxml2-dev           \
      ninja-build           \
      pkg-config            \
      python2               \
      python-six            \
      python2-dev           \
      python3-six           \
      python3-distutils     \
      rsync                 \
      swig                  \
      systemtap-sdt-dev     \
      tzdata                \
      unzip                 \
      uuid-dev

ENV PATH "$PATH:/app/protoc/linux64"
WORKDIR /app
COPY --from=build /out ./
RUN chmod -R +x /app/protoc/linux64
ENTRYPOINT ["dotnet", "ProtocApi.dll"]