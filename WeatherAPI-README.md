# Weather API - .NET Core 8

## Overview
Production-ready Weather API built with .NET Core 8.

## Features
- RESTful API endpoints
- JWT Authentication
- AES-256 Encryption
- Audit Logging
- Swagger Documentation

## Getting Started

```bash
dotnet restore WeatherAPI/WeatherAPI.csproj
dotnet build WeatherAPI/WeatherAPI.csproj
dotnet run --project WeatherAPI/WeatherAPI.csproj
```

## API Endpoints

- GET /api/weather/{city} - Get weather data
- POST /api/weather - Create weather data (requires auth)

## Security
- JWT Bearer token authentication
- AES-256 encryption for sensitive data
- Comprehensive audit logging