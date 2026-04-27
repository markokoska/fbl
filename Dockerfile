# Stage 1: Build React frontend
# Using full Debian-based node image (not alpine) — alpine misses glibc which
# breaks several Vite/Tailwind native binaries.
FROM node:20 AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
# `--legacy-peer-deps` skips strict peer-dep resolution (Vite 8 vs Tailwind's
# declared peer range). Lockfile already pins working versions.
RUN npm install --no-audit --no-fund --legacy-peer-deps
COPY frontend/ ./
RUN npm run build

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /backend
COPY backend/FBL.Api/FBL.Api.csproj ./
RUN dotnet restore
COPY backend/FBL.Api/ ./
RUN dotnet publish -c Release -o /publish

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-build /publish ./
# Copy React build into wwwroot so .NET serves it as static files
COPY --from=frontend-build /frontend/dist ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "FBL.Api.dll"]
