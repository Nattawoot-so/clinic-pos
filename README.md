# Clinic POS Platform

A multi-tenant, multi-branch Clinic POS system built with .NET 10, Next.js, PostgreSQL, Redis, and RabbitMQ.

## Prerequisites

- **Docker Desktop** (v4.x+) with Docker Compose v2
- **Git**
- Ports **3000**, **5000**, **5432**, **5672**, **6379**, **15672** must be available

## Quick Start

```bash
git clone https://github.com/Nattawoot-so/clinic-pos.git
cd clinic-pos
docker compose up --build
```

Wait until all 5 services are healthy (first build takes ~2-3 minutes). Database migrations and seed data are applied automatically on startup.

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Seeded Users

| Username | Password   | Role   | Description |
|----------|------------|--------|-------------|
| admin    | admin123   | Admin  | Can do everything: create patients, appointments, manage users |
| user     | user123    | User   | Can create patients and appointments, view data |
| viewer   | viewer123  | Viewer | Read-only: can view patients and appointments, cannot create |

All users belong to **"Downtown Clinic Group"** tenant with 2 branches: **Main Street Branch** and **Eastside Branch**.

## Architecture Overview

### Tenant Isolation Strategy (Section E2)

This system uses a **discriminator column pattern** for multi-tenant data isolation:

1. **TenantId on every entity**: All tenant-scoped entities (Patient, Branch, User, Appointment) have a `TenantId` column.

2. **EF Core Global Query Filters**: Every read query is automatically filtered by `TenantId`. This is configured in `ClinicDbContext.OnModelCreating()`:
   ```csharp
   modelBuilder.Entity<Patient>().HasQueryFilter(p => p.TenantId == _tenantId);
   ```
   This prevents developers from accidentally forgetting tenant filters.

3. **Write Isolation**: `TenantId` is **always derived from JWT claims** (via `ITenantContext`), never accepted from client requests. This prevents tenant spoofing.

4. **How TenantId is derived**:
   - User logs in → JWT token contains `tenant_id` claim
   - Every request includes JWT in `Authorization: Bearer` header
   - `HttpTenantContext` extracts `tenant_id` from the authenticated user's claims
   - `ClinicDbContext` captures this in its constructor for query filter use

5. **Preventing accidental missing filters**:
   - Global query filters are applied automatically on all LINQ queries
   - Only `IgnoreQueryFilters()` can bypass them (used only in login and seeding)
   - Code review flag: any use of `IgnoreQueryFilters()` should be carefully reviewed

**Why not Row-Level Security (RLS)?** Added complexity at the DB layer without proportional benefit for v1. Global query filters provide equivalent protection at the application layer with better testability.

**Why not database-per-tenant?** Operational overhead (migrations, backups, connection management) is too high for a POS platform. The discriminator column pattern is the pragmatic choice for v1.

### Patient-Branch Relationship

Using `PrimaryBranchId` (nullable FK on Patient) instead of a separate mapping table. This is sufficient for v1 where we only need to know the patient's primary registration branch. A `PatientBranch` mapping table can be added later when visit history tracking is needed.

## Assumptions and Trade-offs

- **Auth is simplified**: JWT-based, no refresh tokens or password reset. Sufficient for demonstrating RBAC enforcement.
- **No pagination**: Patient/Appointment lists return all results. Would add cursor-based pagination for production.
- **Role check is inline**: Using `tenant.Role == "Viewer"` checks in endpoints instead of ASP.NET policy framework. Simpler for the timebox.
- **Redis is in docker-compose** but not actively used for caching (Section D was not selected). It's ready for future cache implementation.
- **MassTransit consumer logs events** — in production, this would trigger notifications, analytics, etc.

## API Examples

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Create Patient
```bash
curl -X POST http://localhost:5000/api/patients \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"firstName":"John","lastName":"Doe","phoneNumber":"081-234-5678"}'
```

### List Patients
```bash
curl http://localhost:5000/api/patients \
  -H "Authorization: Bearer <TOKEN>"
```

### List Patients by Branch
```bash
curl "http://localhost:5000/api/patients?branchId=<BRANCH_ID>" \
  -H "Authorization: Bearer <TOKEN>"
```

### Create Appointment
```bash
curl -X POST http://localhost:5000/api/appointments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"patientId":"<PATIENT_ID>","branchId":"<BRANCH_ID>","startAt":"2026-03-01T10:00:00Z"}'
```

### List Branches
```bash
curl http://localhost:5000/api/branches \
  -H "Authorization: Bearer <TOKEN>"
```

### Create User (Admin only)
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"username":"newuser","password":"pass123","role":"User"}'
```

### Assign Role (Admin only)
```bash
curl -X PUT http://localhost:5000/api/users/<USER_ID>/role \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"role":"Admin"}'
```

## Environment Variables

See `.env.example` for all required environment variables. Docker Compose sets these automatically.

## Running Tests

```bash
# Option 1: If .NET 10 SDK is installed locally
cd src/backend
dotnet test

# Option 2: Using Docker (no .NET SDK required)
docker run --rm -v "$(pwd)/src/backend:/app" -w /app mcr.microsoft.com/dotnet/sdk:10.0 dotnet test
```

### Test Coverage

| Test | What it verifies |
|------|-----------------|
| `Patients_Are_Isolated_By_Tenant` | Patient created in Tenant A is invisible to Tenant B query |
| `Same_Phone_Allowed_Across_Different_Tenants` | Same phone number can exist in different tenants |
| `Cannot_Create_Duplicate_Phone_In_Same_Tenant` | Unique index on (TenantId, PhoneNumber) is configured |
| `Unique_Index_Scoped_To_Tenant_And_Phone` | Model metadata confirms composite unique index |
| `Create_And_List_Patients_Integration` | Full HTTP flow: create patient → list patients → verify result |

## Tech Stack

- **Backend**: .NET 10 / C# Minimal API
- **Frontend**: Next.js 15 + Tailwind CSS 4
- **Database**: PostgreSQL 17 with EF Core 10
- **Cache**: Redis 7 (infrastructure ready)
- **Messaging**: RabbitMQ 3 with MassTransit

## Sections Completed

- [x] **Section A**: Core slice (Create Patient, List Patients) — backend + frontend
- [x] **Section B**: Authorization (JWT, RBAC: Admin/User/Viewer), User management, Seeder
- [x] **Section C**: Appointments + duplicate prevention + RabbitMQ event publishing
- [x] **Section E2**: Tenant isolation strategy (this README)
