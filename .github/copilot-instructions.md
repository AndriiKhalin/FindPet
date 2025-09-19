# GitHub Copilot Instructions for FindPet Project

## 🎯 Project Overview

**FindPet** is a comprehensive full-stack web platform for finding lost pets, built with modern technologies and clean architecture principles. The platform integrates machine learning for automatic pet breed recognition and provides a robust system for pet owners and finders to connect.

### Core Functionality
- 🔐 JWT-based authentication and role-based authorization
- 📝 Pet advertisement management (create, search, manage lost/found pets)
- 🤖 ML.NET integration for automatic pet breed recognition from photos
- 📱 Responsive user interface with modern UI/UX
- 🔍 Advanced search with filters and geolocation
- 👥 User management system with roles
- 📸 Image upload and processing capabilities
- 🗺️ Location-based pet search (planned)
- 💬 Real-time notifications via SignalR (planned)

## 🛠 Technology Stack

### Backend (FindPet_API)
- **Framework**: ASP.NET Core 8.0 with C# 12
- **Database**: SQL Server with Entity Framework Core 8.0.2 (Code-First)
- **Authentication**: ASP.NET Core Identity + JWT Bearer tokens
- **Machine Learning**: ML.NET 3.0.1 with Microsoft.ML.Vision
- **Object Mapping**: AutoMapper 12.0.1
- **Logging**: NLog.Extensions.Logging 5.3.11
- **API Documentation**: Swagger/OpenAPI
- **Testing Framework**: xUnit/MSTest (structure prepared)

### Frontend (FindPet_UI)
- **Framework**: Angular 20.3.0 with TypeScript
- **Styling**: SCSS + Tailwind CSS 3.4.4 + Bootstrap 5.3.8
- **UI Components**: Angular Material 20.2.3
- **Icons**: FontAwesome 6.5.2
- **HTTP Client**: Angular HttpClient with RxJS 7.8.0
- **Forms**: Reactive Forms with validation
- **Internationalization**: intl-tel-input 19.5.7
- **JWT Handling**: @auth0/angular-jwt 5.2.0
- **Server-Side Rendering**: Angular SSR

## 📁 Architecture & Project Structure

### Backend Architecture (Clean Architecture)
```
FindPet_API/
├── FindPet.API/                 # Presentation Layer
│   ├── Controllers/             # REST API controllers
│   ├── Configurations/          # Service configurations
│   ├── Migrations/             # EF Core migrations
│   ├── Program.cs              # Application entry point
│   └── wwwroot/                # Static files
├── FindPet.Core/               # Business Logic Layer
│   ├── Services/               # Business logic services
│   ├── Repositories/           # Repository implementations
│   ├── Mappings/              # AutoMapper profiles
│   └── Helpers/               # Utility classes
├── FindPet.Domain/             # Domain Layer
│   ├── Entities/              # Domain models
│   ├── DTOs/                  # Data Transfer Objects
│   └── ValueObjects/          # Value objects
├── FindPet.Infrastructure/     # Infrastructure Layer
│   ├── Data/                  # DbContext and configurations
│   ├── Interfaces/            # Interface definitions
│   └── ExternalServices/      # External service integrations
└── FindPet.Tests/             # Test projects
```

### Frontend Architecture
```
FindPet_UI/src/app/
├── components/                 # Reusable components
├── pages/                     # Page components
├── services/                  # API services
├── interfaces/                # TypeScript interfaces
├── guards/                    # Route guards
├── interceptor/               # HTTP interceptors
└── environments/              # Environment configs
```

## 🎨 Coding Standards & Patterns

### Backend (C#) Standards
- **Naming**: PascalCase for classes/methods, camelCase for fields/variables
- **Architecture**: Clean Architecture with DDD principles
- **Dependency Injection**: Built-in DI container for all dependencies
- **Error Handling**: Centralized error handling through middleware
- **Async/Await**: Asynchronous operations for all I/O operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **AutoMapper**: Object-to-object mapping
- **Validation**: DataAnnotations and custom validators

### Frontend (TypeScript/Angular) Standards
- **Style Guide**: Angular official style guide
- **Component Architecture**: Smart/Dumb components pattern
- **State Management**: Services with RxJS observables
- **Type Safety**: Strict TypeScript configuration
- **Reactive Forms**: For all forms with comprehensive validation
- **Standalone Components**: Modern Angular standalone architecture
- **Lazy Loading**: Route-based code splitting
- **OnPush**: Change detection strategy for performance

### Design Patterns Used
#### Backend Patterns
- **Repository Pattern**: Data access abstraction (`IBaseRepository<T>`)
- **Unit of Work**: Transaction coordination (`IUnitOfWork`)
- **Service Layer**: Business logic encapsulation
- **Dependency Injection**: Inversion of Control
- **Strategy Pattern**: ML service implementations
- **Factory Pattern**: Entity creation (planned)

#### Frontend Patterns
- **Observable Pattern**: RxJS for asynchronous operations
- **Reactive Programming**: Reactive Forms and HTTP handling
- **Component Communication**: Input/Output properties, services
- **Route Guards**: Authentication and authorization
- **Interceptors**: HTTP request/response processing
- **Container/Presenter**: Smart vs Dumb components

## 🔧 Build & Development Commands

### Backend Commands
```bash
# Build solution
dotnet build FindPet_API.sln

# Run development server
dotnet run --project FindPet.API

# Run tests
dotnet test FindPet.Tests

# Database migrations
dotnet ef database update --project FindPet.API
dotnet ef migrations add [MigrationName] --project FindPet.API

# Restore packages
dotnet restore
```

### Frontend Commands
```bash
# Install dependencies
npm install

# Development server
ng serve

# Production build
ng build --configuration production

# Run tests
ng test

# Lint check
ng lint

# Server-side rendering
npm run serve:ssr:FindPet_UI
```

## 🚀 Priority Improvements & Features

### Critical Fixes Required
1. **Code Cleanup**: Remove commented code in `Program.cs`
2. **Global Error Handling**: Implement centralized exception middleware
3. **Model State Validation**: Add comprehensive input validation
4. **Configuration Management**: Externalize connection strings and secrets
5. **CORS Security**: Implement restrictive CORS policies
6. **Entity Relationships**: Complete EF Core relationship configurations

### Architecture Enhancements
1. **CQRS with MediatR**: Implement command/query separation
2. **FluentValidation**: Replace DataAnnotations with FluentValidation
3. **Redis Caching**: Implement distributed caching
4. **API Versioning**: Add versioning strategy
5. **Rate Limiting**: Implement request throttling
6. **Health Checks**: Add application health monitoring

### New Features to Implement
1. **Real-time Features**:
   - SignalR integration for live notifications
   - Live chat between users
   - Real-time pet match alerts

2. **AI/ML Enhancements**:
   - Azure Custom Vision integration
   - Confidence scoring for predictions
   - Multiple model support for different pet types
   - Auto-tagging from images

3. **User Experience**:
   - Interactive maps (Google Maps/Leaflet)
   - Advanced search with filters
   - Mobile-responsive design improvements
   - Progressive Web App (PWA) capabilities

4. **Security & Performance**:
   - Refresh token implementation
   - Image compression and optimization
   - Database query optimization
   - CDN integration for static assets

5. **Administration**:
   - Admin dashboard
   - User management interface
   - Content moderation tools
   - Analytics and reporting

## 📝 Code Generation Guidelines

### When creating new Controllers:
- Follow existing pattern from `PetController.cs`
- Implement proper HTTP status codes
- Use async/await for all operations
- Include proper exception handling
- Add XML documentation comments
- Use dependency injection for services

### When creating new Services:
- Implement corresponding interface first
- Follow repository pattern for data access
- Use AutoMapper for object mapping
- Implement proper logging with NLog
- Handle exceptions gracefully
- Use async/await consistently

### When creating new Components (Angular):
- Use standalone component architecture
- Implement OnPush change detection
- Follow reactive forms pattern
- Use proper TypeScript typing
- Implement proper error handling
- Add accessibility attributes

### When creating new DTOs:
- Follow existing naming conventions
- Add data annotations for validation
- Include XML documentation
- Ensure proper property mapping in AutoMapper
- Consider versioning for API evolution

## 🎯 Machine Learning Integration

### Current ML.NET Implementation
- **Model**: PetMLModel.mlnet for pet breed classification
- **Service**: MLService.cs handles prediction logic
- **Integration**: Automatic breed detection during pet creation
- **Training**: Custom training pipeline with image classification

### ML Enhancement Roadmap
- Azure Custom Vision integration
- Confidence threshold implementation
- Multi-model support (breed, color, size detection)
- Continuous learning pipeline
- A/B testing for model performance

## 🔍 Testing Strategy

### Backend Testing
- **Unit Tests**: Service layer and business logic
- **Integration Tests**: API controllers and database operations
- **Performance Tests**: ML model inference times
- **Security Tests**: Authentication and authorization

### Frontend Testing
- **Component Tests**: Individual component functionality
- **Service Tests**: API service integration
- **E2E Tests**: Complete user workflows
- **Accessibility Tests**: WCAG compliance

## 🚧 Known Technical Debt

1. **Repository Location**: Move repository implementations from Core to Infrastructure
2. **File Storage**: Replace wwwroot storage with cloud storage (Azure Blob/AWS S3)
3. **Configuration**: Implement Options pattern for settings
4. **Error Handling**: Standardize error response format
5. **Logging**: Migrate from NLog to Serilog for structured logging
6. **Testing**: Increase test coverage to >80%

## 💡 Copilot Usage Guidelines

### For Best Results:
1. **Context**: Always provide context about the layer you're working in
2. **Patterns**: Reference existing patterns in the codebase
3. **Standards**: Follow established naming and coding conventions
4. **Dependencies**: Use existing dependencies rather than introducing new ones
5. **Testing**: Generate corresponding tests for new functionality

### Code Examples to Reference:
- **Controller Pattern**: `PetController.cs`
- **Service Pattern**: `PetService.cs`
- **Repository Pattern**: `PetRepository.cs`
- **DTO Pattern**: `PetDto.cs`
- **Angular Component**: `RegisterComponent.ts`
- **Angular Service**: `AuthService.ts`

This project demonstrates modern full-stack development with clean architecture, ML integration, and scalable design patterns. Use these guidelines to maintain consistency and quality when generating or reviewing code.