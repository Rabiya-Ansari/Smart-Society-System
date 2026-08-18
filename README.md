# SmartSociety

## Housing Society Management System

SmartSociety is a web-based Housing Society Management System designed
to centralize common residential society operations. It provides
role-based access for **Administrators, Residents, Security Staff, and
Maintenance Staff**.

## Main Features

-   ASP.NET Core Identity authentication and role-based authorization
-   Resident onboarding and flat management
-   Resident vehicle management with unique registration numbers
-   Visitor management with server-generated six-digit gate passes
-   Security gate verification and gate logs
-   Complaint submission, assignment, and maintenance workflow
-   Maintenance bills, bill items, and supported payment recording
-   Amenity management and booking with overlap validation
-   Notices and polling
-   Emergency contacts and family members
-   Audit logging
-   Administrative, resident, security, and maintenance workflows
-   Responsive web interface

## User Roles

### Administrator

-   Manage flats and residents
-   Manage users and roles
-   Manage visitors and gate logs
-   Manage complaints and assign maintenance staff
-   Manage maintenance bills and bill items
-   Manage amenities, notices, and polls
-   View audit logs

### Resident

-   Manage profile information
-   Manage vehicles
-   Create visitor requests
-   Submit and track complaints
-   View bills for their own flat
-   Record supported payments
-   Book amenities
-   View notices and vote in polls
-   Manage emergency contacts and family members

### Security Staff

-   Access the security dashboard
-   Verify visitor gate passes
-   View relevant gate information and gate logs

### Maintenance Staff

-   View complaints assigned to them
-   Update assigned complaint status/work information
-   Work only on complaints assigned to their account

## Technology Stack

**Backend** - ASP.NET Core MVC - .NET 10 - ASP.NET Core Identity -
Entity Framework Core

**Database** - Microsoft SQL Server - Entity Framework Core Code First /
Migrations

**Frontend** - Razor Views - HTML5 - CSS3 - Bootstrap 5 - JavaScript -
jQuery

## Architecture

``` text
+---------------------------------------------+
|              UI / Presentation              |
|      Razor Views, HTML, CSS, Bootstrap      |
+---------------------------------------------+
                      |
                      v
+---------------------------------------------+
|           Business / Application            |
|     Controllers, Services, Models,          |
|             ViewModels                      |
+---------------------------------------------+
                      |
                      v
+---------------------------------------------+
|               Data Access                   |
|     Entity Framework Core, DbContext,       |
|              LINQ, Migrations               |
+---------------------------------------------+
                      |
                      v
+---------------------------------------------+
|                SQL Server                   |
|          SmartSociety Database              |
+---------------------------------------------+
```

## Database

The system uses Microsoft SQL Server with Entity Framework Core. Major
data areas include:

-   Users and Roles
-   Flats
-   Resident Profiles
-   Vehicles
-   Visitors
-   Gate Logs
-   Complaints
-   Maintenance Bills
-   Bill Items
-   Payments
-   Amenities
-   Amenity Bookings
-   Notices
-   Polls
-   Poll Options
-   Poll Votes
-   Emergency Contacts
-   Family Members
-   Audit Logs

## Requirements

Before running the project, install:

-   .NET 10 SDK
-   Microsoft SQL Server
-   Visual Studio or another .NET 10-compatible IDE
-   Modern web browser

## Installation

### 1. Get the Project

Clone or copy the project to your local machine.

``` bash
git clone <your-repository-url>
cd SmartSociety
```

If the project is already downloaded, open its folder.

### 2. Configure SQL Server

Open `appsettings.json` and configure the SQL Server connection string.

Example:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SmartSociety;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Use your actual SQL Server configuration. Do not commit production
passwords or secrets to source control.

### 3. Restore Packages

``` bash
dotnet restore
```

### 4. Build the Project

``` bash
dotnet build
```

### 5. Run the Application

``` bash
dotnet run
```

You can also open the project in Visual Studio and select **Run**.

The application startup process initializes the required
database/migrations and development seed data according to the project
configuration.

### 6. Open the Application

Open the localhost URL displayed in the terminal or IDE, for example:

``` text
https://localhost:xxxx
```

The exact port depends on the local project configuration.

## First-Time Setup

1.  Start SQL Server.
2.  Configure the connection string.
3.  Run the application.
4.  Sign in with the development Administrator account.
5.  Create at least one Flat.
6.  Create a Resident and assign the available Flat.
7.  Add demonstration data such as Amenities, Notices, and Polls.
8.  Test Visitor Management and Gate Pass Verification.
9.  Test Complaint and Maintenance workflows.
10. Test Maintenance Billing and Payment workflows.
11. Test Amenity Booking and Polling.
12. Review Audit Logs using the Administrator account.

## Development Accounts

  Role                Development Email
  ------------------- ------------------------------------------
  Administrator       admin@smartsociety.com
  Security Staff      guard@smartsociety.com
  Maintenance Staff   staff@smartsociety.com
  Resident            Created through Administrator onboarding

**Important:** Passwords are intentionally not included in this README.
Use the project's seed configuration or development instructions to
obtain or reset development passwords.

These accounts are for development/testing only and must not be used as
production credentials.

## Security

SmartSociety uses:

-   ASP.NET Core Identity authentication
-   Role-based authorization
-   Server-side validation
-   Ownership checks for resident resources
-   Anti-forgery protection for protected form operations
-   Unique database constraints where required
-   Audit logging for selected operations

For production deployment:

-   Replace development credentials
-   Use strong unique passwords
-   Protect connection strings and secrets
-   Enable HTTPS
-   Review authorization policies
-   Configure database backups
-   Do not commit sensitive configuration to source control

## Important Implementation Notes

-   The implemented visitor workflow uses a **server-generated six-digit
    numeric gate pass**.
-   Payment functionality should be described as supported/simulated
    unless a real third-party payment provider has been separately
    configured.
-   MFA should not be considered enabled unless a separate MFA workflow
    has been configured.
-   Photo upload/storage should not be claimed unless a corresponding
    implementation is added.
-   Live SMS, email, or push notifications require separate provider
    configuration if needed.

## Testing

The project should be tested for:

-   Authentication and login
-   Role-based authorization
-   Resident onboarding
-   Flat management
-   Vehicle uniqueness
-   Visitor creation
-   Gate-pass verification
-   Gate logs
-   Complaint submission and assignment
-   Maintenance staff authorization
-   Maintenance bills
-   Payment validation
-   Amenity booking conflicts
-   Notice publication and expiry
-   Poll voting restrictions
-   Emergency contacts and family members
-   Audit log access
-   Responsive UI
-   Database migrations
-   Application build and startup

### Build Test

``` bash
dotnet build
```

Expected result:

``` text
Build succeeded.
```

## Recommended Demo Flow

``` text
Login
  ↓
Admin Dashboard
  ↓
Resident / Flat Management
  ↓
Visitor Creation
  ↓
Six-Digit Gate Pass
  ↓
Security Verification
  ↓
Complaint Submission
  ↓
Admin Assignment
  ↓
Maintenance Staff Update
  ↓
Maintenance Bill
  ↓
Resident Payment
  ↓
Amenity Booking
  ↓
Notice / Poll
  ↓
Audit Logs
```

## Project Structure

A typical ASP.NET Core MVC organization is:

``` text
SmartSociety/
│
├── Controllers/
├── Models/
├── Views/
├── Services/
├── Data/
├── Migrations/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── Areas/
├── appsettings.json
├── Program.cs
└── README.md
```

The exact structure may vary according to the final project source.

## Future Enhancements

-   QR-code based visitor passes
-   Multi-factor authentication
-   Real payment gateway integration
-   Downloadable payment receipts
-   Secure photo/document uploads
-   Email/SMS/push notifications
-   Advanced analytics and reporting
-   Production monitoring
-   Automated database backups

## Project Information

**Project Name:** SmartSociety\
**Project Type:** Housing Society Management System\
**Platform:** Web Application\
**Framework:** ASP.NET Core MVC / .NET 10\
**Database:** Microsoft SQL Server\
**ORM:** Entity Framework Core

## Conclusion

SmartSociety provides a centralized platform for managing major
residential society operations. Through role-based access, resident
self-service, visitor and gate management, complaint workflows,
maintenance billing, amenity booking, notices, polling, and audit
logging, the system aims to make society management more organized,
secure, and efficient.
