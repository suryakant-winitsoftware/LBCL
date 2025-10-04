# Use the .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
# Copy all project files
COPY Modules/**/*.csproj ./
RUN for file in $(find . -name '*.csproj'); do mkdir -p ${file%/*} && mv $file ${file%/*}/; done


#RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
COPY . .

WORKDIR "/app/Web/WINITWeb"
#RUN dotnet build "WinIt.csproj" -c Release -o /app/build
# Publish the application
RUN dotnet restore "WinIt.csproj" 
RUN dotnet publish "WinIt.csproj" -c Release -o /app/publish 



# Ensure the nginx.conf file is copied from the source directory
#RUN ls -al  # List contents to verify location of nginx.conf
FROM nginx:1.23.0-alpine
WORKDIR /app
EXPOSE 8080
COPY /Web/WINITWeb/nginx.conf /etc/nginx/nginx.conf 
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html


# Optional: list the contents of /etc/nginx to verify the file is there (for debugging)
#RUN ls /etc/nginx

# Default command to run the application (adjust if necessary)
# CMD ["nginx", "-g", "daemon off;"]
