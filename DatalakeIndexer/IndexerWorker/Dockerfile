FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGET_ARCH=arm64

RUN apt update
RUN apt install -y clang zlib1g-dev

WORKDIR /source

COPY *.csproj ./app/

WORKDIR /source/app
RUN dotnet restore -a $TARGET_ARCH

WORKDIR /source

COPY . ./app/

WORKDIR /source/app
RUN dotnet publish -a $TARGET_ARCH -c Release -o /app --self-contained

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled

WORKDIR /app
COPY --from=build /app ./

USER $APP_UID
ENTRYPOINT ["./IndexerWorker"]