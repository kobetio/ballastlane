# My Library - Master Specification

## 1. Project Overview

Develop a full-stack web application named **My Library** that allows
authenticated users to manage their personal book collection.

### Technology Stack

**Backend** - .NET 10 - ASP.NET Core Web API - Entity Framework Core
(Code First) - PostgreSQL - JWT Authentication - Swagger / OpenAPI

**Frontend** - Angular 21 - Angular Material - Reactive Forms - Signals

**Testing** - xUnit - FluentAssertions - Moq

------------------------------------------------------------------------

## 2. User Story

**As a registered user,**

I want to manage my personal library,

**So that** I can organize the books I own, the books I'm reading, and
the books I want to read.

------------------------------------------------------------------------

## 3. Functional Requirements

-   Register
-   Login
-   JWT Authentication
-   Create, Read, Update and Delete books
-   Each authenticated user can access only their own books.

------------------------------------------------------------------------

## 4. Business Rules

### Required

-   Title
-   Author

### Optional

-   Genre
-   PublicationYear
-   ReadingStatus
-   Rating
-   Notes

### Rules

-   Title: max 150 characters.
-   Author: max 100 characters.
-   Genre: max 50 characters.
-   PublicationYear: between 1450 and current year.
-   Rating: 1 to 5.
-   Notes: max 1000 characters.
-   Users can only access their own books.
-   Accessing another user's book must return HTTP 403 Forbidden.

------------------------------------------------------------------------

## 5. Backend Requirements

-   .NET 10
-   Clean Architecture
-   SOLID
-   PostgreSQL
-   Entity Framework Core
-   Code First + Migrations
-   FluentValidation
-   Global Exception Middleware
-   Dependency Injection
-   async/await
-   JWT Authentication
-   Swagger with XML comments, request/response documentation and JWT
    support.

------------------------------------------------------------------------

## 6. Frontend Requirements

-   Angular 21
-   Angular Material
-   Standalone Components
-   Responsive
-   Signals
-   Computed Signals
-   Reactive Forms
-   FormBuilder
-   Typed Forms
-   OnPush Change Detection
-   @if, @for and @switch syntax
-   inject() API
-   Route Guards
-   HTTP Interceptor
-   Smart/Dumb Components when appropriate
-   Feature-based folder organization
-   No business logic inside HTML templates
-   Business logic in TypeScript
-   Use getters when appropriate
-   SnackBar for notifications
-   Dialog confirmation before deletion
-   Loading and Empty states
-   Follow Angular Style Guide

Display API validation messages exactly as returned whenever possible.

------------------------------------------------------------------------

## 7. API Design Guidelines

-   RESTful endpoints
-   Proper HTTP verbs
-   Proper status codes
-   Friendly validation messages
-   Consistent error responses
-   403 Forbidden for unauthorized resource ownership

------------------------------------------------------------------------

## 8. Testing Requirements

Follow TDD using: - xUnit - FluentAssertions - Moq

Cover Domain, Application, Validators, Controllers, Authentication and
Repository/Integration tests.

------------------------------------------------------------------------

## 9. Technical Requirements

Backend: - .NET 10 - PostgreSQL - Entity Framework Core - JWT -
Swagger - FluentValidation

Frontend: - Angular 21 - Angular Material - Signals - Reactive Forms

------------------------------------------------------------------------

## 10. General Development Guidelines

-   Follow SOLID.
-   Follow Clean Architecture.
-   Follow Clean Code.
-   Keep Controllers thin.
-   Separate responsibilities.
-   Avoid duplicated code.
-   Build readable and maintainable code.

------------------------------------------------------------------------

## 11. AI Development Instructions

Generate production-quality code following every requirement in this
document.

Projects: - MyLibrary.Domain - MyLibrary.Application -
MyLibrary.Infrastructure - MyLibrary.Api - MyLibrary.Tests
