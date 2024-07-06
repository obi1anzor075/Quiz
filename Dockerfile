# Use the official .NET SDK image for the build stage
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

# Use the official ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from the publish stage
COPY --from=publish /app/publish .

# Copy certificates and update CA certificates
COPY --from=build /root/.aspnet/https/aspnetapp.pfx /root/.aspnet/https/aspnetapp.pfx
COPY --from=build /root/.aspnet/https/aspnetapp.crt /root/.aspnet/https/aspnetapp.crt

# Install NGINX
RUN apt-get update && apt-get install -y nginx

# Remove the default NGINX configuration file
RUN rm /etc/nginx/nginx.conf

# Copy custom NGINX configuration files
COPY nginx.conf /etc/nginx/nginx.conf
COPY default.conf /etc/nginx/conf.d/default.conf

# Expose ports
EXPOSE 80
EXPOSE 443

# Start NGINX
CMD ["nginx", "-g", "daemon off;"]
