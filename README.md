# SmartSociety

Secure ASP.NET Core MVC society-management system aligned with the supplied SRS.

## Stack
- ASP.NET Core MVC / .NET 10
- ASP.NET Core Identity + Roles
- Entity Framework Core 10
- SQL Server
- Bootstrap 5 + responsive custom UI

## Roles
| Role | Main access |
|---|---|
| Admin | Full administration, resident onboarding, flats, vehicles, visitors, gate logs, bills, amenities, notices, polls, complaints, audit logs |
| Resident | Own profile, vehicles, visitors/gate passes, complaints, bills/payments, amenities/bookings, notices, polls, emergency contacts, family members |
| SecurityStaff | Security dashboard, visitor verification, visitors, gate logs |
| MaintenanceStaff | Assigned complaints only; status/work notes/SLA workflow |

## Accounts
Admin is intentionally seeded because it is a system administration account:
- Email: `admin@smartsociety.com`
- Password: `Admin@12345`

System staff are seeded for testing:
- Security: `guard@smartsociety.com` / `Guard@12345`
- Maintenance: `staff@smartsociety.com` / `Staff@12345`

**Residents are not hardcoded.** The Admin creates residents from `Residents -> Create`. The created Identity account receives the `Resident` role and can immediately log in with the email/password entered by the Admin.

## First run
1. Set the SQL Server connection string in `appsettings.json`.
2. Run the application.
3. The application applies pending EF migrations on startup.
4. Roles, Admin, SecurityStaff and MaintenanceStaff accounts are seeded.
5. Admin creates a Flat.
6. Admin creates a Resident and assigns the available Flat.
7. Resident logs in using the credentials created by Admin.

## Security rules implemented
- Server-side role authorization; UI hiding is not relied upon for security.
- Resident records are filtered by the authenticated user's ResidentProfile.
- Residents cannot submit another FlatId/ResidentProfileId to take ownership of another user's data.
- Visitor gate passes for residents are generated as unique 6-digit codes server-side.
- Resident visitor approval and flat ownership are protected server-side.
- Amenity booking overlap validation is server-side.
- Payment ownership is checked against the resident's Flat before saving.
- Complaint access is restricted to owner/admin/assigned maintenance staff.
- Maintenance staff can only set complaints to `InProgress` or `Resolved` and add WorkNotes.
- Audit logs are Admin-only and paginated/filterable.
- Public Identity registration is disabled because resident onboarding is controlled by Admin.

## Important SRS features
- Resident dashboard
- Admin dashboard
- Security gate terminal and pass verification
- Visitor entry/exit gate logs
- Complaint workflow with WorkNotes and SLA target date
- Maintenance bills and simulated payments
- Amenity booking conflict prevention
- Notices and resident polling
- Emergency contacts and family members
- Sitemap
- Audit logging

## Testing order
1. Admin login
2. Create Flat
3. Create Resident
4. Log out and log in as Resident
5. Create Vehicle
6. Create Visitor and verify generated gate pass
7. Book Amenity
8. Create Complaint
9. Create Maintenance Bill as Admin and pay it as Resident
10. Create Notice/Poll as Admin and view/vote as Resident
11. Log in as Security and verify the visitor pass
12. Log in as Maintenance and update an assigned complaint
13. Check Audit Logs as Admin
14. Try direct URLs from the wrong role and confirm Access Denied/Forbidden
