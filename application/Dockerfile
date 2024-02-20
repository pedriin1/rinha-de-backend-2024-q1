# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["rinha-de-backend-2024-q1.csproj", "./"]
RUN dotnet restore "./rinha-de-backend-2024-q1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "rinha-de-backend-2024-q1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "rinha-de-backend-2024-q1.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "rinha-de-backend-2024-q1.dll"]
