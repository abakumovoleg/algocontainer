#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["Algo.Strategies.Execution.Api/Algo.Strategies.Execution.Api.csproj", "Algo.Strategies.Execution.Api/"]
COPY ["./nuget.config", "Algo.Strategies.Execution.Api/"]
RUN dotnet restore "Algo.Strategies.Execution.Api/Algo.Strategies.Execution.Api.csproj"
COPY . .
WORKDIR "/src/Algo.Strategies.Execution.Api"
RUN dotnet build "Algo.Strategies.Execution.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Algo.Strategies.Execution.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Algo.Strategies.Execution.Api.dll"]