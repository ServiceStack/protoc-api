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
ENV PATH "$PATH:/app/protoc/linux64"
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "ProtocApi.dll"]