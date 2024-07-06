# Use the official .NET SDK image for the build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef

# Make sure the dotnet tools are in the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj files and restore the dependencies
COPY DataAccessLayer/DataAccessLayer.csproj DataAccessLayer/
COPY BusinessLogicLayer/BusinessLogicLayer.csproj BusinessLogicLayer/
COPY PresentationLayer/PresentationLayer.csproj PresentationLayer/
RUN dotnet restore PresentationLayer/PresentationLayer.csproj

# Copy the rest of the project files
COPY . .

# Build the project
WORKDIR /src/PresentationLayer
RUN dotnet build PresentationLayer.csproj -c Release -o /app/build

# Generate and export HTTPS certificate
RUN dotnet dev-certs https -ep /root/.aspnet/https/aspnetapp.pfx -p quiz192837465
RUN dotnet dev-certs https --export-path /root/.aspnet/https/aspnetapp.crt --format PEM

# Publish the project
FROM build AS publish
RUN dotnet publish PresentationLayer.csproj -c Release -o /app/publish

# Use the official ASP.NET runtime image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /root/.aspnet/https/aspnetapp.pfx /root/.aspnet/https/aspnetapp.pfx
COPY --from=build /root/.aspnet/https/aspnetapp.crt /etc/ssl/certs/aspnetapp.crt

# Install CA certificates package and update certificates
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

# Configure Kestrel to use the HTTPS certificate
ENV ASPNETCORE_URLS="https://+:5001;http://+:5000"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=quiz192837465

ENTRYPOINT ["dotnet", "PresentationLayer.dll"]

ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8
