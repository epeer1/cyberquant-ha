# FSScore Assignment - Grades DB System

A complete implementation of the FSScore home assignment featuring a Grades database system with REST API for questions management and reporting.

## Tech Stack & Architecture Decisions

### .NET Web API Framework 4.8
Chose .NET Framework 4.8 Web API as specified in requirements. This provides stable, enterprise-ready web services with JSON support and proper HTTP status codes.

### SQL Server Express 2019 + SSMS
- **SQL Server Express 2019**: Free, lightweight database engine perfect for development
- **SSMS**: Essential for database management, running scripts, and testing stored procedures
- Easy backup restore and query execution

### Project Structure
Created 2 separate projects within the solution:

1. **`FSScore.Database`** - Contains SQL scripts for sections 1 & 2
   - Keeps database logic separate and organized
   - Easy to deploy scripts independently
   - Clear separation of concerns

2. **`FSScore.WebApi`** - Contains API implementation for sections 3, 4 & 5
   - Standard Web API project structure
   - Layered architecture with Controllers, Services, Models, DataAccess

### Dapper for Data Access
Chose Dapper as the Object-Relational Mapper (ORM) because:
- **Performance**: Micro-ORM with minimal overhead, faster than Entity Framework
- **SQL Control**: Direct SQL queries give precise control over database operations
- **Stored Procedure Support**: Perfect fit for assignment's Part 2 stored procedures
- **Simplicity**: Lightweight, easy to use with existing database schema
- **Raw SQL**: Enables complex queries needed for report calculations
- **Async Support**: Modern async/await patterns for better scalability

### Layered Architecture Pattern
Implemented Controller → Service → Repository pattern because:
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: Business logic can be unit tested without HTTP or database
- **Maintainability**: Changes in one layer don't affect others
- **Industry Standard**: Common enterprise pattern for Web APIs

**Note**: Reports skip the Repository layer because they use complex stored procedures and one-off queries that don't benefit from the abstraction overhead of a repository pattern.

### Custom Dependency Injection
Implemented DI pattern similar to .NET Core because:
- .NET Framework 4.8 Web API lacks built-in DI container
- Modern development practices require proper dependency management
- Enables testability and loose coupling
- Provides singleton/transient lifetime management

## Database Setup

1. Install SQL Server Express 2019
2. Install SQL Server Management Studio (SSMS)
3. Restore `GRADES.bak` file to create the Grades database
4. Verify connection string in `FSScore.WebApi/Web.config`:
   ```xml
   <connectionStrings>
     <add name="GradesDB"
          connectionString="Server=localhost\SQLEXPRESS;Database=Grades;Trusted_Connection=True;MultipleActiveResultSets=true"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
5. Run scripts in order:
   - `FSScore.Database/Scripts/simple_migration_script.sql`
   - `FSScore.Database/Scripts/data_generator.sql` (optional)
   - `FSScore.Database/Scripts/calculate_score_per_snapshot.sql`

## API Endpoints

### Base URL: `https://localhost:44355`

### Questions CRUD (Section 3)

#### Get All Questions
```http
GET /api/questions/snapshot/{snapshotId}
```
**Example**: `GET /api/questions/snapshot/1001191`

#### Get Single Question
```http
GET /api/questions/snapshot/{snapshotId}/question/{questionId}
```
**Example**: `GET /api/questions/snapshot/1001191/question/7022`

#### Create Question
```http
POST /api/questions/snapshot/{snapshotId}
Content-Type: application/json

{
  "QuestionId": 155930,
  "QuestionText": "What is 2+2?",
  "Score": null,
  "IsRelevant": true,
  "TestId": 78
}
```

#### Update Question
```http
PUT /api/questions/snapshot/{snapshotId}/question/{questionId}
Content-Type: application/json

{
  "QuestionText": "Updated question text",
  "Score": 85,
  "IsRelevant": true,
  "TestId": 78
}
```

#### Delete Question
```http
DELETE /api/questions/snapshot/{snapshotId}/question/{questionId}
```

### Reports API

#### Student Report (Section 4)
```http
GET /api/reports/student/{snapshotId}
```
**Example**: `GET /api/reports/student/1001191`

**Response**:
```json
{
  "Success": true,
  "Message": "Student report generated for snapshot 1001191",
  "Data": {
    "Title": "Student report",
    "CreationDate": "2025-08-01T11:21:15.8671907+03:00",
    "SnapshotId": 1001191,
    "TopZones": [
      {
        "ZoneId": 447,
        "ZoneName": "zone number:1",
        "Score": 79.00,
        "TotalQuestions": 46,
        "AnsweredQuestions": 32
      }
    ],
    "BottomZones": [...],
    "LowScoreZones": [...]
  }
}
```

#### Principal Report (Section 5)
```http
POST /api/reports/principal
Content-Type: application/json

{
  "SnapshotIds": [1001190, 1001191]
}
```

**Response**:
```json
{
  "Success": true,
  "Message": "Principal report generated across 2 snapshots",
  "Data": {
    "Title": "Principal Report",
    "CreationDate": "2025-08-01T11:14:41.7342448+03:00",
    "LowestAverageZone": {
      "ZoneId": 456,
      "ZoneName": "zone number:10",
      "Score": 0.00,
      "TotalQuestions": 5,
      "AnsweredQuestions": 4
    },
    "AnalyzedSnapshots": [1001190, 1001191]
  }
}
```

## Testing the API

### 1. Start the Application
- Open solution in Visual Studio
- Set `FSScore.WebApi` as startup project
- Press F5 to run (starts on `https://localhost:44355`)

### 2. Browser Testing (GET endpoints)
Simply paste these URLs in your browser:
- **Database test**: `https://localhost:44355/api/values/test-db`
- **Get questions**: `https://localhost:44355/api/questions/snapshot/1001191`
- **Student report**: `https://localhost:44355/api/reports/student/1001191`

### 3. Postman Testing (Recommended)
1. Download and install Postman
2. Import or manually create requests:
   - **POST Question**: `POST https://localhost:44355/api/questions/snapshot/1001191`
   - **PUT Question**: `PUT https://localhost:44355/api/questions/snapshot/1001191/question/7022`
   - **DELETE Question**: `DELETE https://localhost:44355/api/questions/snapshot/1001191/question/7022`
   - **Principal Report**: `POST https://localhost:44355/api/reports/principal`
3. Set `Content-Type: application/json` header for POST/PUT requests

### 4. PowerShell Testing (Alternative)
```powershell
# Create question
$headers = @{'Content-Type' = 'application/json'}
$body = @{
    QuestionId = 155931
    QuestionText = "Test Question"
    Score = $null
    IsRelevant = $true
    TestId = 78
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44355/api/questions/snapshot/1001191" -Method POST -Headers $headers -Body $body

# Principal Report
$reportBody = @{
    SnapshotIds = @(1001190, 1001191)
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44355/api/reports/principal" -Method POST -Headers $headers -Body $reportBody
```

## Implementation Notes

- **Section 2 Assumption**: The stored procedure `CalculateScorePerSnapshot` is designed to be run manually by the user via SSMS or SQL scripts, not called from the API
- **Section 4 Assumption**: Zone scores calculated using Section 2 methodology (average of relevant questions) as referenced by "calculated scores (section 2)" in the assignment
- Proper NULL handling for unanswered questions
- Score validation (0-100, non-negative)
- Comprehensive error handling with HTTP status codes
- Consistent JSON API response format