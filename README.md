# Manage Your Projects (MYP)

> A modern, full-stack productivity application for developers built with .NET 8 and Angular 17+

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17+-DD0031?logo=angular)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## Overview

**Manage Your Projects** is a personal productivity suite designed for developers. It combines essential tools in a single, secure, and modern platform:

- **Snippets Manager** - Store and search your reusable code snippets with syntax highlighting
- **Kanban Board** - Organize your projects with drag & drop task management
- **Audit Logs** - Track all changes with detailed history

### Why This Project?

This project demonstrates enterprise-level architecture patterns and best practices:

| Pattern | Implementation |
|---------|----------------|
| **Clean Architecture** | 4-layer separation (Domain, Application, Infrastructure, API) |
| **CQRS** | Command/Query segregation with MediatR |
| **Repository Pattern** | Abstracted data access |
| **Smart/Dumb Components** | Presentational vs Container components in Angular |
| **Signals** | Modern Angular state management |

---

## Tech Stack

### Backend
- **.NET 8** - Web API
- **Entity Framework Core 8** - ORM with Fluent API
- **PostgreSQL 15** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **JWT Bearer** - Authentication
- **Serilog** - Structured logging

### Frontend
- **Angular 17+** - SPA framework
- **Signals** - Reactive state management
- **Angular CDK** - Drag & Drop
- **Tailwind CSS** - Styling (Dark Mode)
- **ngx-highlightjs** - Syntax highlighting

### Infrastructure
- **Docker** - Containerization
- **GitHub Actions** - CI/CD

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Angular 17+ SPA                         │
│              (Signals + Smart/Dumb Components)                  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ REST API (HTTPS)
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                          .NET 8 API                             │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                   API Layer                              │   │
│  │         Controllers • Middleware • Swagger               │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                Application Layer                         │   │
│  │       Commands • Queries • Handlers • Validators         │   │
│  │                    (MediatR)                             │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  Domain Layer                            │   │
│  │          Entities • Value Objects • Interfaces           │   │
│  │                (Zero Dependencies)                       │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               Infrastructure Layer                       │   │
│  │        EF Core • Repositories • JWT • Services           │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
                    ┌───────────────────┐
                    │   PostgreSQL 15   │
                    └───────────────────┘
```

---

## Database Schema

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│    Users     │       │   Snippets   │       │     Tags     │
├──────────────┤       ├──────────────┤       ├──────────────┤
│ PK Id        │◄──┐   │ PK Id        │◄─────►│ PK Id        │
│    Email     │   │   │ FK UserId    │───┐   │ FK UserId    │───┐
│    Password  │   │   │    Title     │   │   │    Name      │   │
│    Name      │   │   │    Code      │   │   │    Color     │   │
└──────────────┘   │   │    Language  │   │   └──────────────┘   │
                   │   └──────────────┘   │                      │
                   │                      │                      │
┌──────────────┐   │   ┌──────────────┐   │                      │
│   Projects   │   │   │   Columns    │   │                      │
├──────────────┤   │   ├──────────────┤   │                      │
│ PK Id        │◄──┼───│ FK ProjectId │   │                      │
│ FK UserId    │───┤   │    Name      │   │                      │
│    Name      │   │   │    Order     │   │                      │
│    Color     │   │   └──────────────┘   │                      │
└──────────────┘   │          │           │                      │
                   │          ▼           │                      │
                   │   ┌──────────────┐   │                      │
                   │   │  TaskItems   │   │                      │
                   │   ├──────────────┤   │                      │
                   │   │ PK Id        │   │                      │
                   │   │ FK ColumnId  │   │                      │
                   │   │    Title     │   │                      │
                   │   │    Priority  │   │                      │
                   │   │    DueDate   │   │                      │
                   │   └──────────────┘   │                      │
                   │                      │                      │
                   │   ┌──────────────┐   │                      │
                   │   │  AuditLogs   │   │                      │
                   │   ├──────────────┤   │                      │
                   └───│ FK UserId    │   │                      │
                       │    Entity    │   │                      │
                       │    Action    │   │                      │
                       │    Changes   │ (JSONB)                  │
                       └──────────────┘                          │
                                                                 │
                            N:N Relations ◄──────────────────────┘
                         (SnippetTags, TaskLabels)
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/products/docker-desktop)

### Quick Start (Docker)

```bash
# Clone the repository
git clone https://github.com/yourusername/manage-your-projects.git
cd manage-your-projects

# Start everything with Docker
docker-compose up -d

# Application available at:
# - Frontend: http://localhost:4200
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
```

### Development Setup

```bash
# Backend
cd src/MYP.API
dotnet restore
dotnet run

# Frontend (in another terminal)
cd src/myp-client
npm install
ng serve
```

### Environment Variables

Create a `.env` file in the root:

```env
# Database
POSTGRES_USER=myp
POSTGRES_PASSWORD=your_secure_password
POSTGRES_DB=myp_db

# JWT
JWT_SECRET=your_jwt_secret_min_32_characters
JWT_ISSUER=MYP
JWT_AUDIENCE=MYP

# API
ASPNETCORE_ENVIRONMENT=Development
```

---

## Project Structure

```
manage-your-projects/
├── src/
│   ├── MYP.Domain/           # Entities, Value Objects, Interfaces
│   ├── MYP.Application/      # Commands, Queries, Handlers, DTOs
│   ├── MYP.Infrastructure/   # EF Core, Repositories, Services
│   ├── MYP.API/              # Controllers, Middleware, Config
│   └── myp-client/           # Angular frontend
│
├── tests/
│   ├── MYP.Domain.Tests/
│   ├── MYP.Application.Tests/
│   ├── MYP.Infrastructure.Tests/
│   └── MYP.API.Tests/
│
├── docs/
│   ├── SPECS.md              # Functional specifications
│   ├── ARCHITECTURE.md       # Technical architecture
│   ├── ROADMAP.md            # Development roadmap
│   ├── TESTING.md            # Testing strategy
│   └── CONVENTIONS.md        # Code conventions
│
├── docker-compose.yml
├── Dockerfile
└── README.md
```

---

## API Documentation

Interactive API documentation is available via Swagger at `/swagger` when running the application.

### Main Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Authenticate user |
| GET | `/api/snippets` | List user's snippets |
| POST | `/api/snippets` | Create snippet |
| GET | `/api/snippets/search` | Full-text search |
| GET | `/api/projects` | List user's projects |
| POST | `/api/projects` | Create project |
| PUT | `/api/tasks/{id}/move` | Move task (drag & drop) |
| GET | `/api/audit-logs` | View audit history |

---

## Testing

```bash
# Run all backend tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/MYP.Application.Tests

# Run frontend tests
cd src/myp-client
ng test --code-coverage
```

### Test Coverage Goals

- Unit Tests: > 80% coverage
- Integration Tests: All API endpoints
- Boundary Tests: All validators

---

## Why CQRS?

The project uses **CQRS (Command Query Responsibility Segregation)** pattern for several reasons:

1. **Separation of Concerns** - Read and write operations have different requirements
2. **Scalability** - Can optimize reads and writes independently
3. **Testability** - Each handler is an isolated, testable unit
4. **Maintainability** - One file = one action, easy to understand
5. **Validation** - FluentValidation integrated via Pipeline Behaviors

```csharp
// Example: Creating a snippet
public record CreateSnippetCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; }
    public string Code { get; init; }
    public string Language { get; init; }
}

// Validation runs automatically via MediatR pipeline
// Handler focuses only on business logic
```

---

## Screenshots

> *Screenshots will be added as features are implemented*

---

## Roadmap

- [x] Project documentation
- [ ] Phase 0: Infrastructure setup
- [ ] Phase 1: Authentication module
- [ ] Phase 2: Snippets module
- [ ] Phase 3: Kanban module
- [ ] Phase 4: Audit logs
- [ ] Phase 5: Polish & testing
- [ ] Phase 6: Deployment

See [ROADMAP.md](docs/ROADMAP.md) for detailed breakdown.

---

## Contributing

This is a portfolio project, but suggestions and feedback are welcome! Feel free to open an issue.

---

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

## Author

**[Your Name]**

- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your Profile](https://linkedin.com/in/yourprofile)

---

*Built with passion to demonstrate enterprise-level .NET and Angular development skills.*
