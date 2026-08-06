FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WppSender.sln ./
COPY src/WppSender.Domain/WppSender.Domain.csproj src/WppSender.Domain/
COPY src/WppSender.Application/WppSender.Application.csproj src/WppSender.Application/
COPY src/WppSender.Infrastructure/WppSender.Infrastructure.csproj src/WppSender.Infrastructure/
COPY src/WppSender.Api/WppSender.Api.csproj src/WppSender.Api/
RUN dotnet restore src/WppSender.Api/WppSender.Api.csproj

COPY src/ src/
RUN dotnet publish src/WppSender.Api/WppSender.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WppSender.Api.dll"]
