# ServiceName

A minimal API project created from the MsMinimalTemplate.

## Project Structure
```
ServiceName/
└── ServiceName.Api/     # Main API project
    ├── ServiceName.Api.csproj
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

## Features
- .NET 8 Minimal API
- Swagger/OpenAPI documentation
- Health check endpoint
- Sample CRUD endpoints
- Configuration support

## Endpoints

### Health Check
- `GET /health` - Returns service health status

### Sample API
- `GET /api/hello?name={name}` - Returns a greeting message
- `POST /api/items` - Creates a new item

## Running the Application

1. Navigate to the project directory:
   ```bash
   cd ServiceName/ServiceName.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to `https://localhost:5001/swagger` to view the API documentation.

## Configuration

The application uses standard .NET configuration patterns. You can modify settings in:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development settings

## Development

This minimal API provides a starting point for building lightweight web APIs with .NET 8. You can extend it by:
- Adding more endpoints
- Implementing data persistence
- Adding authentication and authorization
- Integrating with external services