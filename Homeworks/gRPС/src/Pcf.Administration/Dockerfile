# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and project files, then restore dependencies
COPY Pcf.Administration/*.sln ./
COPY Pcf.Administration/*/*.csproj ./
COPY Pcf.Common.Events/*.csproj ./Pcf.Common.Events/
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done
RUN dotnet restore

# Copy the remaining files and publish the application
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet publish Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj -c Release -o out

# Use the .NET 8 runtime for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.Administration.WebHost.dll"]