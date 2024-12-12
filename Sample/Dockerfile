ARG LAUNCHING_FROM_VS
ARG FINAL_BASE_IMAGE=${LAUNCHING_FROM_VS:+aotdebug}

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StepCa.Client/StepCa.Client.csproj", "StepCa.Client/"]
COPY ["Sample/Sample.csproj", "Sample/"]
RUN dotnet restore "./Sample/Sample.csproj"
COPY ["Sample", "Sample"]
COPY ["StepCa.Client", "StepCa.Client"]
WORKDIR "/src/Sample"
RUN dotnet build "./Sample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Sample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS aotdebug
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb
USER app

FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:8.0} AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./Sample"]