# Asset Management System

A comprehensive asset management solution developed for healthcare organizations using ASP.NET Core MVC. This application streamlines the complete lifecycle management of assets from acquisition to disposal.

## Features

- **Complete Asset Lifecycle Management:**
 - Asset acquisition and registration
 - Asset transfers between departments
 - Maintenance tracking
 - Disposal documentation and processing
 - Return document generation

- **Hierarchical Location Management:**
 - Facilities
 - Buildings
 - Floors
 - Rooms
 - Departments

- **Role-Based Access Control:**
 - Admin: Complete system access
 - Manager: Department management and reporting
 - Supervisor: Asset supervision and approval
 - Data Entry: Basic asset data management

- **Real-Time Dashboard:**
 - Asset count statistics
 - Recent activity monitoring
 - Department-wise asset distribution
 - Status-based filtering

- **Document Generation:**
 - PDF generation for asset transfers
 - Disposal documentation
 - Return forms
 - Automated email notifications

- **Audit and Tracking:**
 - Complete change logging
 - User activity tracking
 - Detailed audit trails

## Technology Stack

- **Backend:** ASP.NET Core MVC (.NET 9.0)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript, Bootstrap
- **Authentication:** ASP.NET Core Identity
- **PDF Generation:** QuestPDF
- **Design Patterns:** 
 - Repository Pattern
 - Unit of Work
 - Dependency Injection

## Architecture

The application follows a modular N-tier architecture:

- **Presentation Layer (PL):** MVC controllers, views, and view models
- **Business Logic Layer (BLL):** Services, repositories, and business logic
- **Data Access Layer (DAL):** Entity models and database context

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or later
