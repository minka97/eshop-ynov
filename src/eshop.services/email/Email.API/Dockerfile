FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.services/email/Email.API/Email.API.csproj", "eshop.services/email/Email.API/"]
COPY ["eshop.shared/blocks/BuildingBlocks/BuildingBlocks.csproj", "eshop.shared/blocks/BuildingBlocks/"]
COPY ["eshop.shared/blocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "eshop.shared/blocks/BuildingBlocks.Messaging/"]
COPY ["eshop.services/ordering/Ordering.Domain/Ordering.Domain.csproj", "eshop.services/ordering/Ordering.Domain/"]
RUN dotnet restore "eshop.services/email/Email.API/Email.API.csproj"
COPY . .
WORKDIR "/src/eshop.services/email/Email.API"
RUN dotnet build "Email.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/eshop.services/email/Email.API"
RUN dotnet publish "Email.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Email.API.dll"]
