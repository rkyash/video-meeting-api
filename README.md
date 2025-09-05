# Video Meeting API - Clean Architecture with Minimal APIs

A modern video meeting application built with **Clean Architecture**, **Minimal APIs**, **ASP.NET 9**, **PostgreSQL**, **Scalar UI**, and **Vonage Video SDK**.

## 🏗️ Architecture Overview

This project implements **Clean Architecture** with **Minimal APIs** and the following layers:

```
VideoMeeting.Solution/
├── src/
│   ├── VideoMeeting.Domain/          # Domain Layer (Entities, Value Objects, Enums)
│   ├── VideoMeeting.Application/     # Application Layer (Use Cases, Interfaces, DTOs)
│   ├── VideoMeeting.Infrastructure/  # Infrastructure Layer (Data Access, External Services)
│   └── VideoMeeting.Api/            # Presentation Layer (Minimal APIs, Endpoints)
├── tests/                           # Test Projects
└── VideoMeeting.sln                 # Solution File
```

### 📋 Architecture Principles

- **Clean Architecture**: Dependency inversion and separation of concerns
- **Minimal APIs**: Modern, lightweight HTTP APIs with top-level programs
- **Domain-Centric**: Business logic is isolated in the Domain layer
- **CQRS Pattern**: Using MediatR for command/query separation
- **Scalar UI**: Modern, beautiful API documentation interface

## 🚀 Features

### 🎥 Core Video Meeting Features
- ✅ Create and manage video meetings
- ✅ Join meetings as authenticated users or guests
- ✅ Real-time video conferencing with Vonage Video SDK
- ✅ Screen sharing capabilities
- ✅ Meeting recording functionality
- ✅ Room codes for easy joining

### 🔐 Authentication & Security
- ✅ JWT-based authentication with Minimal APIs
- ✅ User registration and login endpoints
- ✅ Password hashing with BCrypt
- ✅ Role-based participant management

### 🏛️ Modern Architecture Features
- ✅ Clean Architecture with separate layers
- ✅ **Minimal APIs** for lightweight HTTP endpoints
- ✅ **Scalar UI** for beautiful API documentation
- ✅ CQRS with MediatR
- ✅ Domain-Driven Design patterns
- ✅ Entity Framework Core with PostgreSQL
- ✅ Dependency Injection

## 🛠️ Technology Stack

| Layer | Technologies |
|-------|-------------|
| **Domain** | .NET 9, C# Records, Value Objects |
| **Application** | MediatR, FluentValidation, DTOs |
| **Infrastructure** | Entity Framework Core, PostgreSQL, Vonage SDK, BCrypt |
| **API** | **Minimal APIs**, JWT Bearer, SignalR, **Scalar UI** |

### 📦 Key NuGet Packages

```xml
<!-- Application Layer -->
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />

<!-- Infrastructure Layer -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="OpenTok" Version="3.18.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />

<!-- API Layer -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Scalar.AspNetCore" Version="1.2.5" />
```

## 🏃‍♂️ Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Vonage Video API Account](https://tokbox.com/)

### 1. Clone the Repository
```bash
git clone <repository-url>
cd VideoMeeting.Solution
```

### 2. Configure Database
Update connection string in `src/VideoMeeting.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=videomeeting_dev_db;Username=your_username;Password=your_password"
  }
}
```

### 3. Configure Vonage Credentials
Add your Vonage API credentials:
```json
{
  "Vonage": {
    "ApiKey": "your-vonage-api-key",
    "ApiSecret": "your-vonage-api-secret"
  }
}
```

### 4. Restore Dependencies
```bash
dotnet restore
```

### 5. Run Database Migrations
```bash
cd src/VideoMeeting.Infrastructure
dotnet ef migrations add InitialCreate -s ../VideoMeeting.Api
dotnet ef database update -s ../VideoMeeting.Api
```

### 6. Run the Application
```bash
cd ../VideoMeeting.Api
dotnet run
```

### 7. Access Scalar API Documentation
Navigate to: `https://localhost:7000` (or the port shown in console)

The beautiful **Scalar UI** will be displayed at the root URL showing all your API endpoints with interactive documentation.

## 📖 Minimal API Endpoints

### 🔐 Authentication Endpoints (`/api/auth`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | User login |
| GET | `/api/auth/profile` | Get user profile |
| POST | `/api/auth/change-password` | Change password |

### 🎥 Meeting Endpoints (`/api/meetings`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/meetings` | Create meeting |
| GET | `/api/meetings/{id}` | Get meeting by ID |
| GET | `/api/meetings/room/{code}` | Get meeting by room code |
| GET | `/api/meetings/my-meetings` | Get user's meetings |
| POST | `/api/meetings/{id}/join` | Join meeting (authenticated) |
| POST | `/api/meetings/{id}/join-as-guest` | Join as guest |
| GET | `/api/meetings/{id}/participants` | Get meeting participants |
| POST | `/api/meetings/{id}/recordings/start` | Start recording |
| GET | `/api/meetings/{id}/recordings` | Get meeting recordings |

### 📊 System Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/api/info` | API information |

## 🏗️ Minimal API Architecture

### 🌐 Endpoint Organization (`VideoMeeting.Api/Endpoints/`)

```
Api/Endpoints/
├── AuthEndpoints.cs            # Authentication endpoints
├── MeetingEndpoints.cs         # Meeting management endpoints
└── [Future endpoints...]       # Extensible endpoint modules
```

### 📋 Endpoint Module Example

```csharp
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created);

        // ... more endpoints
    }
}
```

### 🎯 Benefits of Minimal APIs

- **Performance**: Reduced overhead compared to MVC controllers
- **Simplicity**: Less boilerplate code, more focused endpoints
- **Modern**: Latest .NET approach for HTTP APIs
- **Testability**: Easy to test individual endpoint handlers
- **Maintainability**: Organized in feature-specific modules

## 🎨 Scalar UI Features

### ✨ What makes Scalar UI better than Swagger?

- **Beautiful Design**: Modern, clean interface with dark/light themes
- **Better UX**: Improved navigation and endpoint organization
- **Enhanced Documentation**: Rich markdown support and better formatting
- **Interactive Testing**: Built-in API testing with request/response examples
- **Performance**: Faster loading and better performance
- **Customizable**: Easy theming and customization options

### 🎯 Scalar UI Configuration

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Scalar UI at root URL
}
```

### 📱 Accessing Scalar UI

1. Run the application: `dotnet run`
2. Open browser to the displayed URL (typically `https://localhost:7000`)
3. Enjoy the beautiful Scalar API documentation interface!

## 🏛️ Layer Details

### 🌟 Domain Layer (`VideoMeeting.Domain`)
**Pure business logic with no external dependencies**

```
Domain/
├── Common/
│   └── BaseEntity.cs              # Base entity class
├── Entities/
│   ├── User.cs                    # User aggregate root
│   ├── Meeting.cs                 # Meeting aggregate root
│   ├── MeetingParticipant.cs      # Participant entity
│   └── MeetingRecording.cs        # Recording entity
├── ValueObjects/
│   ├── Email.cs                   # Email value object
│   └── RoomCode.cs                # Room code value object
└── Enums/
    └── MeetingStatus.cs           # Domain enums
```

### 🎯 Application Layer (`VideoMeeting.Application`)
**Use cases, interfaces, and application services**

```
Application/
├── Common/
│   └── Interfaces/                # Abstraction interfaces
├── Features/
│   ├── Authentication/
│   │   ├── Commands/              # Authentication commands
│   │   ├── DTOs/                  # Data transfer objects
│   │   └── Handlers/              # MediatR command handlers
│   └── Meetings/
│       ├── Commands/              # Meeting commands
│       ├── Queries/               # Meeting queries
│       ├── Handlers/              # Query/Command handlers
│       └── DTOs/                  # Meeting DTOs
└── DependencyInjection.cs        # DI configuration
```

### 🔧 Infrastructure Layer (`VideoMeeting.Infrastructure`)
**External concerns like database, external APIs**

```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs    # EF Core context
│   └── Configurations/            # Entity configurations
├── Services/
│   ├── AuthService.cs             # Authentication service
│   └── VonageService.cs           # Vonage SDK service
└── DependencyInjection.cs        # DI configuration
```

### 🌐 API Layer (`VideoMeeting.Api`) - **Minimal APIs**
**HTTP API with Minimal APIs and Scalar UI**

```
Api/
├── Endpoints/
│   ├── AuthEndpoints.cs           # Authentication endpoints
│   └── MeetingEndpoints.cs        # Meeting endpoints
├── Program.cs                     # Minimal API application setup
└── appsettings.json               # Configuration
```

## 🧪 Testing Minimal APIs

### Unit Testing Endpoint Handlers
```csharp
[Test]
public async Task RegisterAsync_Should_Return_Created_When_Valid()
{
    // Arrange
    var mediator = Mock.Of<IMediator>();
    var command = new RegisterUserCommand("test@example.com", "John", "Doe", "password");
    
    // Act
    var result = await AuthEndpoints.RegisterAsync(command, mediator);
    
    // Assert
    Assert.IsType<Created>(result);
}
```

### Integration Testing with TestHost
```csharp
public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Test]
    public async Task RegisterEndpoint_Should_Return_201()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { email = "test@example.com", /* ... */ };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

## 🔄 Development Workflow

### Adding New Minimal API Endpoints

1. **Create Endpoint Module** - Define endpoints in a static class
2. **Application Layer** - Create commands/queries and handlers
3. **Infrastructure** - Implement data access and external services
4. **Register Endpoints** - Map endpoints in Program.cs

### Example: Adding Chat Messages

```csharp
// 1. Endpoint Module
public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat")
            .WithOpenApi();

        group.MapPost("/messages", SendMessageAsync)
            .RequireAuthorization()
            .WithSummary("Send chat message");
    }
    
    private static async Task<IResult> SendMessageAsync(
        SendChatMessageCommand command,
        IMediator mediator)
    {
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }
}

// 2. Register in Program.cs
app.MapChatEndpoints();
```

## 🚀 Deployment

### Docker Support for Minimal APIs
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["VideoMeeting.sln", "."]
# ... rest of Dockerfile
```

### Performance Benefits
- **Faster Startup**: Minimal APIs have less overhead
- **Better Throughput**: Optimized request pipeline
- **Smaller Memory Footprint**: Reduced allocations
- **AOT Ready**: Prepared for Native AOT compilation

## 🎯 Minimal APIs Best Practices

### ✅ Do's
- **Group Related Endpoints**: Use `MapGroup()` for logical organization
- **Use OpenAPI Attributes**: Enhance documentation with `WithSummary()`, `WithDescription()`
- **Leverage Dependency Injection**: Inject services directly into endpoint handlers
- **Keep Handlers Small**: Focus on single responsibility
- **Use Proper HTTP Status Codes**: Return appropriate `Results` types

### ❌ Don'ts
- **Avoid Complex Logic in Handlers**: Use MediatR commands/queries instead
- **Don't Skip Documentation**: Always use OpenAPI attributes
- **Avoid Deep Nesting**: Keep endpoint paths clean and logical
- **Don't Ignore Authentication**: Use `RequireAuthorization()` where needed

## 📋 Roadmap

### Phase 1 - Core Features ✅
- [x] Clean Architecture with Minimal APIs
- [x] User authentication endpoints
- [x] Basic meeting management
- [x] Vonage SDK integration
- [x] Scalar UI documentation

### Phase 2 - Enhanced Features
- [ ] Complete meeting functionality endpoints
- [ ] Real-time chat endpoints
- [ ] Meeting scheduling endpoints
- [ ] Advanced recording endpoints

### Phase 3 - Advanced Features
- [ ] SignalR hubs integration
- [ ] Webhook endpoints for Vonage
- [ ] Analytics endpoints
- [ ] Admin management endpoints

### Phase 4 - Enterprise Features
- [ ] Rate limiting with Minimal APIs
- [ ] API versioning
- [ ] Health checks enhancement
- [ ] Monitoring and metrics endpoints

## 🎨 Scalar UI vs Swagger Comparison

| Feature | Scalar UI ✅ | Swagger UI |
|---------|--------------|------------|
| **Design** | Modern, beautiful | Standard, dated |
| **Performance** | Fast loading | Slower |
| **Themes** | Dark/Light built-in | Limited theming |
| **Navigation** | Intuitive sidebar | Tab-based |
| **Documentation** | Rich markdown | Basic |
| **Testing** | Enhanced UX | Standard |
| **Customization** | Easy to customize | Complex setup |

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- **Documentation**: Scalar UI at the root URL when running
- **Issues**: Create an issue in the repository
- **Discussions**: Use GitHub Discussions for general questions

---

**Built with ❤️ using Clean Architecture, Minimal APIs, and Scalar UI on .NET 9**