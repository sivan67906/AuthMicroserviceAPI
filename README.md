# Authentication Microservice - .NET 9 Enterprise Solution

A complete, production-ready .NET 9 authentication and user management microservice with Blazor WebAssembly client. This solution demonstrates modern enterprise architecture patterns including CQRS, dual database support, vertical slice architecture, and comprehensive authentication features.

## Overview

This enterprise-grade solution provides a complete authentication system with two separate solutions: an API microservice built with ASP.NET Core Web API and a Blazor WebAssembly client application. The architecture follows industry best practices and implements modern design patterns for scalability, maintainability, and testability.

## Architecture

The solution implements Vertical Slice Architecture combined with CQRS pattern, featuring dual database support where commands are written to SQL Server and queries are read from PostgreSQL. This architecture provides clear separation of concerns while maintaining high cohesion within each feature.

### API Microservice Projects

The API solution consists of four core projects organized by responsibility:

**AuthMicroservice.Domain** contains all domain entities including ApplicationUser, ApplicationRole, Address, RefreshToken, and Client. These entities form the core business model and extend ASP.NET Core Identity with Guid-based primary keys.

**AuthMicroservice.Infrastructure** provides data access and external service implementations. It includes dual DbContexts for SQL Server (commands) and PostgreSQL (queries), entity configurations using Fluent API, token generation services, and health check implementations.

**AuthMicroservice.Application** implements business logic using CQRS with MediatR. Features are organized in vertical slices including Register, Login, RefreshToken, ForgotPassword, ResetPassword, and ChangePassword. Each feature contains its command or query, handler, validator, and response DTOs.

**AuthMicroservice.API** serves as the entry point, providing RESTful endpoints through controllers, comprehensive Swagger documentation with JWT support, middleware configuration, and dependency injection setup.

### Blazor Client Projects

The Blazor solution provides the frontend implementation:

**AuthClient.BlazorWasm** is the Blazor WebAssembly application with authentication state management, login and registration pages, JWT token storage using Blazored.LocalStorage, and HTTP client configuration.

**AuthClient.Shared** contains shared DTOs and models used for communication between the client and API.

## Technologies and Patterns

This solution incorporates modern technologies and design patterns:

The architecture uses Vertical Slice Architecture for feature organization and CQRS pattern with MediatR for command and query separation. Dual database support provides SQL Server for write operations and PostgreSQL for read operations.

Authentication and authorization features include JWT Bearer authentication with refresh token rotation, ASP.NET Core Identity with custom Guid-based entities, support for external providers including Google and Microsoft, two-factor authentication capabilities, and all four authorization types: simple authorization, role-based authorization, claims-based authorization, and policy-based authorization.

Development patterns include Result Pattern for consistent error handling, Repository Pattern through Entity Framework Core, FluentValidation for input validation with MediatR pipeline behaviors, Mapster for object mapping, and Serilog for structured logging with console and file sinks.

Quality and monitoring features include health checks for both databases and memory monitoring, global exception handling middleware, comprehensive validation throughout all layers, and HttpClientFactory for HTTP communication.

## Features Implemented

### Authentication Features

The system provides user registration with email validation and password strength requirements, login functionality with JWT access and refresh tokens, token refresh mechanism with rotation for security, password management including forgot password, reset password, and change password operations, email confirmation workflow, and support for two-factor authentication.

### Authorization Features

Authorization is implemented at multiple levels. Simple authorization secures endpoints with the Authorize attribute. Role-based authorization provides Admin, User, and Moderator roles with policy enforcement. Claims-based authorization uses custom claims like Permission for fine-grained control. Policy-based authorization implements complex authorization logic through custom policies.

### Additional Features

The system tracks refresh tokens across multiple clients including Web, Android, and iOS platforms. It manages user addresses with default shipping and billing address support. Profile management allows users to view and update their information. Health check endpoints monitor API and database status. Comprehensive logging captures all operations through Serilog.

## Database Schema

The system uses dual databases with identical schemas but different purposes:

**Command Database (SQL Server)** handles all write operations including user registration, login attempts, token generation, password changes, and profile updates.

**Query Database (PostgreSQL)** handles all read operations with NoTracking for optimal performance including user profile retrieval, address lookups, token validation, and authorization checks.

### Core Tables

The Users table extends ASP.NET Core Identity with Guid IDs and includes fields for full name, profile photo URL, email confirmation status, two-factor authentication settings, and last login timestamp.

The Roles table includes Admin, User, and Moderator roles with descriptions.

The Addresses table stores user addresses with support for multiple addresses per user and default shipping and billing address flags.

The RefreshTokens table tracks refresh tokens with client identification (Web/Android/iOS), user agent and IP tracking, expiration and revocation tracking, and token replacement chain for security.

The Clients table defines application clients (Web, Android, iOS) with activation status.

## Getting Started

### Prerequisites

Before running this solution, ensure you have the following installed:

.NET 9 SDK or later is required for building and running the applications.

SQL Server 2019 or later serves as the command database.

PostgreSQL 14 or later serves as the query database.

Visual Studio 2022 (17.8+) or JetBrains Rider 2024.1+ provides the development environment.

Optionally, Docker Desktop can be used for running databases in containers.

### Database Setup

You can set up databases using Docker for quick setup:

```bash
# SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

# PostgreSQL
docker run --name postgres -e POSTGRES_PASSWORD=YourStrong@Passw0rd \
  -p 5432:5432 -d postgres:15
```

Alternatively, install SQL Server and PostgreSQL directly on your system and update connection strings accordingly.

### Configuration

Update the connection strings in appsettings.json for both databases. The JWT secret key should be changed for production use. External authentication settings for Google and Microsoft should be configured with your own credentials obtained from Google Cloud Console and Azure Portal respectively.

### Running Migrations

Navigate to the API project directory and run migrations for both databases:

```bash
cd src/AuthMicroservice.API

# SQL Server Migrations
dotnet ef migrations add InitialCreate --context CommandDbContext
dotnet ef database update --context CommandDbContext

# PostgreSQL Migrations
dotnet ef migrations add InitialCreate --context QueryDbContext
dotnet ef database update --context QueryDbContext
```

The migrations will create all necessary tables and seed initial data including Admin, User, and Moderator roles, a default admin user with credentials admin@example.com and password Admin@123, and client definitions for Web, Android, and iOS platforms.

### Running the API

Navigate to the API project and run the application:

```bash
cd src/AuthMicroservice.API
dotnet run
```

The API will be available at https://localhost:7000 with Swagger UI accessible at https://localhost:7000/swagger.

### Running the Blazor Client

Navigate to the Blazor project and run the application:

```bash
cd Client/AuthClient.BlazorWasm
dotnet run
```

The Blazor application will be available at https://localhost:7001.

## API Endpoints

### Authentication Endpoints

POST /api/auth/register registers a new user with email, password, and optional full name.

POST /api/auth/login authenticates a user and returns JWT tokens requiring email, password, and client ID.

POST /api/auth/refresh-token refreshes the access token using a refresh token.

POST /api/auth/forgot-password initiates the password reset process.

POST /api/auth/reset-password completes the password reset with a token.

POST /api/auth/change-password changes the password for an authenticated user.

### Authorization Test Endpoints

GET /api/auth/test-auth demonstrates simple authorization requiring any authenticated user.

GET /api/auth/test-admin demonstrates role-based authorization requiring Admin role.

GET /api/auth/test-claims demonstrates claims-based authorization requiring FullAccess claim.

GET /api/auth/test-policy demonstrates policy-based authorization requiring Admin or Moderator role.

### Health Check Endpoints

GET /health provides overall health status.

GET /health/ready checks readiness including database connectivity.

GET /health/live provides liveness probe for Kubernetes deployments.

## Security Considerations

The solution implements multiple security measures:

JWT tokens use a strong secret key with HMAC-SHA256 signing. Access tokens expire after 15 minutes while refresh tokens expire after 7 days. Tokens are stored securely in Blazor client using LocalStorage with HTTPOnly consideration for production.

Password requirements include minimum six characters, at least one uppercase and lowercase letter, one number, and one special character. Passwords are hashed using ASP.NET Core Identity's default hasher.

Refresh tokens implement rotation for security where each refresh generates new tokens and revokes old ones. IP address and user agent are tracked for audit purposes. Tokens can be manually revoked if compromised.

Account lockout activates after five failed login attempts with a 15-minute lockout duration.

## Testing

A test project structure is included with XUnit as the testing framework, FluentAssertions for readable assertions, and Moq for mocking dependencies. The structure supports unit tests for handlers and validators, integration tests for API endpoints, and end-to-end tests for complete workflows.

## Extending the Solution

The architecture makes it easy to add new features:

To add a new authentication feature, create a new folder under Features/Authentication, add command/query classes with validators, implement handlers using dependency injection, register in the controller, and create corresponding Blazor pages if needed.

To implement email confirmation, add email service interface and implementation, create email templates, implement SendConfirmationMail and VerifyMail features, and add email configuration to appsettings.

To add two-factor authentication, enable TwoFactorEnabled in ApplicationUser, implement GenerateTwoFactorToken and VerifyTwoFactorToken, add QR code generation for authenticator apps, and create UI in Blazor for 2FA setup.

## Troubleshooting

### Common Issues

If the database connection fails, verify SQL Server and PostgreSQL are running and check connection strings match your database configuration.

If migrations fail, ensure you are in the API project directory and verify Entity Framework tools are installed.

If JWT validation fails, check that the secret key matches between appsettings and verify token expiration times.

If CORS errors occur, ensure the Blazor client URL is in the CORS policy and check the API base URL in Blazor appsettings.

## Production Considerations

For production deployments, use Azure Key Vault or similar for secrets management. Implement proper SSL/TLS certificates. Use connection string encryption. Set up database backups and monitoring. Configure CDN for Blazor static files. Implement rate limiting and API throttling. Set up centralized logging with services like Application Insights. Configure auto-scaling for high availability.

## License

This project is provided as a reference implementation for educational and commercial use.

## Support

For questions or issues regarding this implementation, please refer to the comprehensive inline documentation throughout the codebase. Each feature includes detailed comments explaining the implementation approach and design decisions.

## Acknowledgments

This solution demonstrates modern enterprise patterns and practices including Clean Architecture principles, Domain-Driven Design concepts, CQRS and Event Sourcing patterns, and OAuth 2.0 and OpenID Connect standards.
