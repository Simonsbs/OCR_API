# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install system dependencies required by IronOCR and Tesseract
RUN apt-get update \
    && apt-get install -y --no-install-recommends apt-utils libgdiplus libc6-dev \
    && apt-get install -y libleptonica-dev libtesseract-dev tesseract-ocr \
    && rm -rf /var/lib/apt/lists/*

# Use SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OCR_API/OCR_API.csproj", "OCR_API/"]
RUN dotnet restore "OCR_API/OCR_API.csproj"
COPY . .
WORKDIR "/src/OCR_API"
RUN dotnet build "OCR_API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "OCR_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OCR_API.dll"]
