FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

COPY *.sln .
COPY LocalStack.Models/*.csproj LocalStack.Models/
COPY LocalStack.Repository/*.csproj LocalStack.Repository/
COPY LocalStack.Services/*.csproj LocalStack.Services/
COPY LocalStack.Api/*.csproj LocalStack.Api/

RUN dotnet restore LocalStackApi.sln

COPY . .
RUN dotnet publish LocalStack.Api/LocalStack.Api.csproj -c Release -o /release

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
COPY --from=build /release ./
ENTRYPOINT ["dotnet", "LocalStack.Api.dll"]
