FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY api.csproj ./
RUN dotnet restore api.csproj

COPY . .
RUN dotnet publish api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:7278
EXPOSE 7278

ENTRYPOINT ["dotnet", "api.dll"]
