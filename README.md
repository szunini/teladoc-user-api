# teladoc-user-api
RESTful API coding exercise for Teladoc â€“ Senior Software Engineer
=======
# Teladoc.Api

## Description

**Teladoc.Api** is a sample project for user management, built with .NET 10 and ready to run in containers. It uses Redis for caching and Entity Framework Core for SQL Server persistence, ensuring fast performance and a modern development experience.

---

## System Overview

A quick look at what you'll find:

- **Teladoc.Api**: RESTful API for user management, with versioning, validations, and robust error handling.
- **Redis**: Improves API performance through caching.
- **Entity Framework Core**: ORM for SQL Server database access and migrations.
- **Swagger**: Interactive API documentation.
- **Unit Tests**: Automated tests to ensure code quality.

---

## Development Environment Requirements

Make sure you have installed:

- **Docker** (optional, for running in containers)
- **.NET 10 SDK**
- **SQL Server** (local or in a container)
- **Redis** (local or in a container)

---

## Running Locally

1. Clone the repository and navigate to the project's root folder.
2. If using Docker, you can start the required services with:

```sh
docker-compose up --build
```


## Testing the API

- Access Swagger documentation at:  
  [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)  
  *(port configurable in `launchSettings.json` or Docker Compose)*

- Example endpoint to create a user (POST `/api/v1/users`):

```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan.perez@example.com",
  "dateOfBirth": "1990-05-15",
  "nickName": "JP",
  "friendCount": 5
}
```

---

## Unit Tests

1. Navigate to the test folder:

```sh
cd ../teladoc.unit.test
```

2. Run the tests:

```sh
dotnet test
```

---

## Possible Improvements

- Implement authentication and authorization.
- Improve test coverage.
- Add Health Checks for external dependencies.
- Deploy to the cloud (Azure, AWS, etc).

---

## Documentation

- Swagger UI: [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)
- Database and Redis configuration in `appsettings.Development.json`.
