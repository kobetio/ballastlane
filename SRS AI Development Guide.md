# My Library

## Software Requirements Specification (SRS) & AI Development Guide

> Version 1.0

------------------------------------------------------------------------

# 1. Purpose

This document defines the functional, technical and architectural
requirements for **My Library**, a full-stack application developed as a
technical interview project.

The objective is to demonstrate:

-   .NET 10
-   Angular 21
-   Clean Architecture
-   SOLID Principles
-   Clean Code
-   Test-Driven Development (TDD)
-   JWT Authentication
-   REST API design
-   Modern Angular best practices
-   Unit testing
-   PostgreSQL
-   Swagger documentation

------------------------------------------------------------------------

# 2. Business Context

My Library is a personal library management application.

Each registered user owns an independent collection of books.

Users must only be able to access their own books.

------------------------------------------------------------------------

# 3. User Story

**As a registered user**

I want to manage my personal book collection

**So that**

I can organize the books I own, books I'm currently reading and books I
plan to read.

------------------------------------------------------------------------

# 4. Scope

## Authentication

-   Register
-   Login
-   JWT Authentication
-   Authorization

## Books

-   Create
-   List
-   Details
-   Update
-   Delete

------------------------------------------------------------------------

# 5. Business Rules

Required fields:

-   Title
-   Author

Optional fields:

-   Genre
-   PublicationYear
-   ReadingStatus
-   Rating
-   Notes

Rules:

-   Title \<=150 characters
-   Author \<=100 characters
-   Genre \<=50 characters
-   PublicationYear between 1450 and current year
-   Rating between 1 and 5
-   Notes \<=1000 characters
-   Books belong to exactly one user
-   Users cannot access another user's books
-   Unauthorized ownership returns **HTTP 403 Forbidden**

------------------------------------------------------------------------

# 6. Solution Architecture

Projects:

-   MyLibrary.Domain
-   MyLibrary.Application
-   MyLibrary.Infrastructure
-   MyLibrary.Api
-   MyLibrary.Tests

Dependency flow:

API -\> Application -\> Domain

Infrastructure implements Application interfaces.

Domain has no external dependencies.

------------------------------------------------------------------------

# 7. Backend Requirements

Technology

-   .NET 10
-   ASP.NET Core Web API
-   Entity Framework Core
-   PostgreSQL
-   Code First
-   Migrations
-   JWT
-   Swagger
-   FluentValidation

Architecture

-   Clean Architecture
-   SOLID
-   Dependency Injection
-   Repository Pattern
-   Thin Controllers
-   Business rules only inside Application
-   Domain independent from Infrastructure
-   Async/Await everywhere
-   CancellationToken support
-   ILogger
-   Options Pattern
-   IConfiguration

Validation

-   FluentValidation
-   Global validation pipeline
-   Friendly validation messages

Exception Handling

-   Global Exception Middleware
-   Standard error responses
-   User-friendly messages

Swagger

-   XML Comments
-   Endpoint documentation
-   Request/Response descriptions
-   Property descriptions
-   Response codes
-   JWT authorization configured

------------------------------------------------------------------------

# 8. Frontend Requirements

Technology

-   Angular 21
-   Angular Material

Angular Best Practices

-   Responsive
-   Standalone Components
-   Feature-based architecture
-   Standalone routing
-   Signals
-   Writable Signals
-   Computed Signals
-   FormBuilder
-   Strongly typed Reactive Forms
-   Reactive Forms only
-   Control Flow syntax (@if, @for, @switch)
-   inject() API when appropriate
-   OnPush Change Detection
-   Smart/Dumb Components when appropriate
-   Business logic only in TypeScript
-   No business logic inside HTML templates
-   Use getters to simplify bindings
-   HTTP Interceptor
-   Route Guards
-   Loading state
-   Empty state
-   Angular Material Dialog
-   Angular Material SnackBar
-   Official Angular Style Guide

UX

-   Responsive layout
-   Angular Material only
-   Display API validation messages exactly as returned
-   Display API error messages exactly as returned
-   Confirmation dialog before delete

------------------------------------------------------------------------

# 9. API Guidelines

Authentication:

-   JWT Bearer

Status Codes

-   200
-   201
-   204
-   400
-   401
-   403
-   404
-   500

Responses

-   Consistent payloads
-   Validation errors
-   Friendly messages
-   ProblemDetails or standardized error model

------------------------------------------------------------------------

# 10. Database

Database:

-   PostgreSQL

Entities

User

-   Id
-   Name
-   Email
-   PasswordHash

Book

-   Id
-   Title
-   Author
-   Genre
-   PublicationYear
-   ReadingStatus
-   Rating
-   Notes
-   UserId

Relationship

User 1:N Books

------------------------------------------------------------------------

# 11. Testing

Follow TDD.

Libraries

-   xUnit
-   FluentAssertions
-   Moq

Tests

-   Domain
-   Application
-   Validators
-   Controllers
-   Authentication
-   Repository
-   Integration tests where appropriate

------------------------------------------------------------------------

# 12. Coding Standards

-   SOLID
-   Clean Architecture
-   Clean Code
-   DRY
-   KISS
-   Meaningful names
-   Small methods
-   Small classes
-   Separation of concerns
-   Dependency Injection
-   No duplicated business logic

------------------------------------------------------------------------

# 13. Folder Organization

Backend

-   Domain
-   Application
-   Infrastructure
-   Api
-   Tests

Frontend

-   core
-   shared
-   features
-   layout
-   models
-   services

------------------------------------------------------------------------

# 14. AI Development Instructions

Generate production-ready code.

Requirements:

-   Follow every section of this specification.
-   Never place business logic inside controllers.
-   Never place business logic inside Angular HTML templates.
-   Keep responsibilities isolated.
-   Generate readable code.
-   Generate XML comments for public backend APIs.
-   Generate Swagger documentation.
-   Produce code without warnings.
-   Prefer maintainability over unnecessary complexity.
-   Respect Angular 21 and .NET 10 best practices.
