FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_ENVIRONMENT=Docker
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

# Copy csproj and restore as distinct layers
# Copy all project files
COPY Modules/**/*.csproj ./
RUN for file in $(find . -name '*.csproj'); do mkdir -p ${file%/*} && mv $file ${file%/*}/; done

# Copy everything else and publish
COPY . .
WORKDIR "/src/WINITAPI"
RUN dotnet restore "WINITAPI.csproj"
RUN dotnet publish "WINITAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WINITAPI.dll"]