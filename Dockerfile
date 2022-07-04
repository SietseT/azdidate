FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG Version
WORKDIR /src
COPY ["Azdidate/Azdidate.csproj", "Azdidate/"]
RUN dotnet restore "Azdidate/Azdidate.csproj" --runtime alpine-x64
COPY . .
WORKDIR "/src/Azdidate"

RUN dotnet publish "Azdidate.csproj" -c Release -o /app/publish \
    --no-restore \
    --runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true \
    /p:Version=$Version \
    /p:InformationalVersion=$Version 

# use different image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final

# create a new user and change directory ownership
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["./azdidate"]
