# FS Score Application Architecture

## 🏗️ Overview
This document outlines the complete architecture for the FS Score Assessment System, a .NET Framework 4.8 Web API application for managing educational assessments with a snapshot-based scoring system.

## 📋 System Requirements Summary
- **Database:** SQL Server 2019 Express with snapshot-based assessment data
- **Backend:** ASP.NET Web API Framework 4.8 with RESTful endpoints
- **Assignment Parts:** SQL scripts (1-2) + REST API (3-5) + Reports (4-5)

## 🏛️ High-Level Architecture

### Application Layers
```
┌─────────────────────────────────────────┐
│             Client Layer                │
│        (Postman, Browser, etc.)        │
└─────────────────────────────────────────┘
                    ↓ HTTP REST
┌─────────────────────────────────────────┐
│           API Controllers               │
│    (QuestionsController, ReportsController) │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│          Business Services              │
│   (QuestionService, ReportService)     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Data Access Layer               │
│         (Dapper + Repository)           │
└─────────────────────────────────────────┘
                    ↓ SQL
┌─────────────────────────────────────────┐
│            Database Layer               │
│     (SQL Server + Stored Procedures)   │
└─────────────────────────────────────────┘
```

## 🗂️ Project Structure

### Solution Organization
```
FSScoreAssignment.sln
├── FSScore.WebApi/                    ← Main Web API Project
│   ├── Controllers/                   ← API Controllers
│   │   ├── QuestionsController.cs
│   │   └── ReportsController.cs
│   ├── Services/                      ← Business Logic Layer
│   │   ├── IQuestionService.cs
│   │   ├── QuestionService.cs
│   │   ├── IReportService.cs
│   │   └── ReportService.cs
│   ├── DataAccess/                    ← Data Access Layer
│   │   ├── IQuestionRepository.cs
│   │   ├── QuestionRepository.cs
│   │   └── DatabaseConnection.cs
│   ├── Models/                        ← Data Transfer Objects
│   │   ├── Question.cs
│   │   ├── Subject.cs
│   │   ├── Zone.cs
│   │   ├── StudentReport.cs
│   │   └── PrincipalReport.cs
│   ├── App_Start/                     ← Configuration
│   └── Web.config                     ← Connection strings
└── FSScore.Database/                  ← Database Scripts Project
    └── Scripts/
        ├── simple_migration_script.sql
        ├── calculate_score_per_snapshot.sql
        └── data_generator.sql
```

## 🛠️ Technology Stack

### Backend Technologies
- **Framework:** ASP.NET Web API Framework 4.8
- **ORM:** Dapper (lightweight, SQL-friendly)
- **Database:** SQL Server 2019 Express
- **JSON:** Newtonsoft.Json (already included)
- **Dependency Injection:** Built-in Web API DI container

### Development Tools
- **IDE:** Visual Studio 2019/2022
- **Database:** SQL Server Management Studio (SSMS)
- **Version Control:** Git + GitHub
- **Testing:** Postman for API testing

## 🗄️ Database Architecture

### Snapshot-Based Design
```
Catalog (SnapshotId = 0)
├── Master question bank
├── All possible subjects, zones, tests
└── NULL scores (template only)

Assessment Snapshots (SnapshotId > 0)
├── Student-specific question subset
├── Actual scores (0-100 or NULL)
├── Customized zone relevance
└── Complete relationship duplication
```

### Core Tables
- **Subjects** - Academic subjects (Math, Science, etc.)
- **Zones** - Sub-topics within subjects (Algebra, Geometry)
- **Questions** - Individual assessment items
- **Tests** - Question collections (National vs Regular)
- **SubjectZones** - Many-to-many: Subjects ↔ Zones
- **ZonesQuestions** - Many-to-many: Zones ↔ Questions

### Stored Procedures
- **CalculateScorePerSnapshot** - Score calculations for reports
- **GenerateTestData** - Automated test data creation (bonus)

## 🌐 API Architecture

### RESTful Design Principles
- **Resource-based URLs** - `/api/questions/{snapshotId}`
- **HTTP verbs** - GET, POST, PUT, DELETE
- **Status codes** - 200, 201, 400, 404, 500
- **JSON payloads** - Consistent request/response format

### Endpoint Design

#### Questions API (Assignment Part 3)
```http
GET    /api/questions/{snapshotId}           → Get all questions for snapshot
POST   /api/questions/{snapshotId}           → Create new question
PUT    /api/questions/{snapshotId}/{id}      → Update question
DELETE /api/questions/{snapshotId}/{id}      → Delete question
```

#### Reports API (Assignment Parts 4-5)
```http
GET    /api/reports/student/{snapshotId}     → Student report
POST   /api/reports/principal                → Principal report (list of snapshots in body)
```

### Data Flow Architecture

#### Questions CRUD Flow
```
Client Request → QuestionsController → QuestionService → QuestionRepository → Database
                                  ↓
Client Response ← JSON Serialization ← Business Logic ← Dapper Mapping ← SQL Results
```

#### Reports Generation Flow
```
Client Request → ReportsController → ReportService → Stored Procedure → Database
                                ↓
Client Response ← Report Model ← Score Calculations ← Raw Data ← SQL Results
```

## 🔧 Service Layer Architecture

### Separation of Concerns
- **Controllers** - HTTP handling, routing, validation
- **Services** - Business logic, orchestration
- **Repositories** - Data access, SQL operations
- **Models** - Data contracts, DTOs

### Business Logic Responsibilities

#### QuestionService
- Question validation (score 0-100, required fields)
- Cascade delete logic (remove from ZonesQuestions first)
- Snapshot existence validation
- Business rule enforcement

#### ReportService
- Score calculation orchestration
- Report formatting and structure
- Multiple snapshot aggregation
- Business intelligence logic

## 📊 Data Models Architecture

### Core Models
```csharp
public class Question
{
    public int SnapshotId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public int? Score { get; set; }        // Nullable for unanswered
    public bool IsRelevant { get; set; }
    public int TestId { get; set; }
}

public class StudentReport
{
    public string ReportTitle { get; set; }
    public DateTime ReportCreationDate { get; set; }
    public List<ZoneScore> TopZones { get; set; }
    public List<ZoneScore> BottomZones { get; set; }
    public List<ZoneScore> LowPerformanceZones { get; set; }
}

public class PrincipalReport
{
    public string ReportTitle { get; set; }
    public DateTime ReportCreationDate { get; set; }
    public ZoneScore LowestAverageZone { get; set; }
}
```

### API Response Patterns
```csharp
// Success Response
{
    "success": true,
    "data": { /* actual data */ },
    "message": "Operation completed successfully"
}

// Error Response
{
    "success": false,
    "error": "Validation failed",
    "details": "Score must be between 0 and 100"
}
```

## 🔒 Error Handling Architecture

### Exception Handling Strategy
- **Global Exception Handler** - Catch unhandled exceptions
- **Custom Exceptions** - Business rule violations
- **Validation Errors** - Input validation failures
- **Database Errors** - Connection, SQL issues

### Error Response Consistency
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public string Error { get; set; }
    public List<string> Details { get; set; }
}
```

## 🔗 Database Connection Architecture

### Connection Management
- **Connection String** - Stored in Web.config
- **Connection Factory** - Centralized connection creation
- **Transaction Management** - For complex operations
- **Connection Pooling** - Automatic via SQL Client

### Dapper Integration
```csharp
// Repository Pattern with Dapper
public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetBySnapshotIdAsync(int snapshotId);
    Task<Question> GetByIdAsync(int snapshotId, int questionId);
    Task<int> CreateAsync(Question question);
    Task<bool> UpdateAsync(Question question);
    Task<bool> DeleteAsync(int snapshotId, int questionId);
}
```

## 🧪 Testing Architecture

### Testing Strategy
- **Unit Tests** - Service layer business logic
- **Integration Tests** - Database operations
- **API Tests** - Controller endpoints via Postman
- **Manual Testing** - SSMS for stored procedures

### Test Data Management
- **Use GenerateTestData procedure** for consistent test data
- **Separate test snapshots** to avoid affecting real data
- **Rollback strategies** for integration tests

## 📈 Performance Considerations

### Database Performance
- **Stored Procedures** - Pre-compiled, faster execution
- **Indexed Queries** - Leverage existing database indexes
- **Minimal Data Transfer** - Only required columns
- **Connection Pooling** - Reuse database connections

### API Performance
- **Async Operations** - Non-blocking database calls
- **Minimal Serialization** - Focused DTOs
- **HTTP Caching** - Cache-Control headers where appropriate
- **Pagination** - For large question lists (future enhancement)

## 🚀 Deployment Architecture

### Development Environment
- **Local SQL Server** - localhost\SQLEXPRESS
- **Local IIS Express** - ASP.NET development server
- **Local Testing** - Postman collections

### Production Considerations (Future)
- **SQL Server Connection** - Production database
- **IIS Hosting** - Windows Server deployment
- **Configuration Management** - Environment-specific settings
- **Logging** - Application and error logging

## 🔄 Implementation Phases

### Phase 1: Foundation ✅ (Complete)
- Database project setup
- SQL scripts (migration, score calculation, data generator)
- Testing and validation

### Phase 2: Core API (Next)
- Dapper setup and configuration
- Data models and repositories
- Questions CRUD API implementation

### Phase 3: Reports API
- Report services implementation
- Student report endpoint
- Principal report endpoint

### Phase 4: Integration & Testing
- End-to-end testing
- Error handling refinement
- Documentation completion

## 🎯 Success Criteria

### Functional Requirements
- ✅ All SQL scripts working and tested
- ✅ Questions CRUD API with proper validation
- ✅ Student report with top/bottom/low zones
- ✅ Principal report with lowest average zone
- ✅ Proper snapshot-aware operations

### Technical Requirements
- ✅ Clean architecture with separated concerns
- ✅ Proper error handling and validation
- ✅ RESTful API design principles
- ✅ Professional code organization
- ✅ Complete GitHub repository

This architecture provides a solid foundation for building a maintainable, scalable assessment system that meets all assignment requirements while following industry best practices. 