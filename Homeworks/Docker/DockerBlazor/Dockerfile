# 1. базовый образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY DockerBlazor.csproj .
RUN dotnet restore DockerBlazor.csproj

# Копируем остальные файлы и публикуем
COPY . .
RUN dotnet publish -c Release -o /app

# 2. финальный рантайм-образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT [ "dotnet", "DockerBlazor.dll" ]
