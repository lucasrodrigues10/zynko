# Stage 1: Build React frontend
FROM node:22-alpine AS node-build
WORKDIR /src/Web/ClientApp
COPY src/Web/ClientApp/package*.json ./
RUN npm ci
COPY src/Web/ClientApp ./
RUN npx vite build

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /repo
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/
COPY src/Shared/Shared.csproj src/Shared/
COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
RUN dotnet restore src/Web/Web.csproj
COPY src/ src/
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish --no-restore -p:SkipWebpackBuild=true
COPY --from=node-build /src/Web/ClientApp/build /app/publish/wwwroot

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=dotnet-build /app/publish ./
ENTRYPOINT ["dotnet", "Zynko.Web.dll"]
