FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /app
COPY ./ExampleGameBackend.sln ./

COPY ./ExampleGameBackend/ExampleGameBackend.csproj ./ExampleGameBackend/ExampleGameBackend.csproj
RUN dotnet restore ./ExampleGameBackend/ExampleGameBackend.csproj

COPY ./ExampleGameBackend ./ExampleGameBackend
RUN dotnet build ./ExampleGameBackend/ExampleGameBackend.csproj -c Release

RUN dotnet publish "./ExampleGameBackend/ExampleGameBackend.csproj" -c Release -o "../../app/out"

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

ENTRYPOINT dotnet ExampleGameBackend.dll