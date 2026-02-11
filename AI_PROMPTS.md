# AI Prompts Used

## Tool Used
**Claude Code** (Claude Opus 4.6) — Anthropic's CLI agent for software engineering

## Prompt 1: Initial Understanding
**Prompt**: "รบกวนอ่านและทำความเข้าใน file ตาม path นี้ให้ที — ก่อนที่ฉันจะสั่งงานให้คุณอ่านให้ละเอียดแล้วอธิบายให้ฉันฟังหน่อย"

**Result**: Accepted — Claude read the PDF and provided a comprehensive summary of the test requirements in Thai, including scenario explanation, section breakdown, and mandatory requirements.

## Prompt 2: Business Model Explanation
**Prompt**: "ก่อนสั่งงาน ฉันยังไม่เข้าใจ business model เลย อธิบายให้เข้าใจหน่อย"

**Result**: Accepted — Claude explained the multi-tenant Clinic POS business model with visual diagrams showing Tenant → Branch → Patient relationships, role-based access control, and data isolation boundaries.

## Prompt 3: Implementation Planning
**Prompt**: "ก่อนอื่นขอแผนที่คุณจะทำก่อนว่าจะต้องทำอะไรบ้าง"

**Result**: Accepted with modifications — Claude generated a comprehensive 13-step implementation plan. I reviewed and approved:
- **Section choice**: C + E2 (recommended by Claude for token efficiency)
- **.NET approach**: Install .NET 10 SDK locally (my choice over Docker-only)
- **Frontend**: Tailwind CSS (my choice over minimal styling)

## Prompt 4: Implementation Execution
**Prompt**: Plan approval triggered Claude to execute all steps sequentially.

### What was accepted:
- **Project structure**: Feature-folder organization (`Features/Auth`, `Features/Patients`, etc.)
- **Tenant isolation**: EF Core Global Query Filters + `ITenantContext` pattern — clean and automatic
- **Auth**: Simple JWT Bearer without Identity framework — appropriate for timebox
- **MassTransit**: Abstraction over RabbitMQ — cleaner than raw `RabbitMQ.Client`
- **Test strategy**: Unit tests for tenant scoping + integration test with `WebApplicationFactory`

### What was modified/iterated:
1. **InMemory DB name in tests**: Claude's first attempt used `$"TestDb_{Guid.NewGuid()}"` inside the `AddDbContext` lambda, which created a new DB per scope. Fixed by pre-computing the name.
2. **MassTransit in tests**: First attempt failed because `WebApplicationFactory` tried to connect to RabbitMQ. Fixed with `AddMassTransitTestHarness()`.
3. **Migration check**: Added `db.Database.IsRelational()` check before calling `MigrateAsync()` to support InMemory provider in tests.
4. **Frontend creation**: `create-next-app` hung on interactive prompt. Switched to manual project setup with explicit `package.json`.

### What was rejected/not used:
- **Clean Architecture layers** (Domain/Application/Infrastructure) — over-engineering for 90-minute timebox
- **ASP.NET Authorization Policies** — inline role checks are simpler and sufficient
- **Redis caching** (Section D) — skipped in favor of Section E2 for better time management
- **Separate mapping table for Patient-Branch** — `PrimaryBranchId` FK is sufficient for v1

## Validation Approach
1. **Backend build verification**: `dotnet build` after each major change to catch compilation errors early
2. **Test-driven validation**: All 5 automated tests pass:
   - Tenant scoping isolation (2 tests)
   - Unique constraint verification (2 tests)
   - Full HTTP integration flow (1 test)
3. **Docker verification**: `docker compose up --build` to verify end-to-end runnability

## Prompt 5: Add Logout
**Prompt**: "Logout ตรงไหน"

**Result**: Accepted with iteration — First attempt placed `"use client"` after `metadata` export in `layout.tsx`, which is invalid in Next.js (server component can't have client directives alongside `metadata`). Fixed by extracting `LogoutButton.tsx` as a separate client component. Later superseded by NavBar component.

## Prompt 6: Fix Role-Based UI
**Prompt**: "ฉัน LOGIN แล้วทั้ง 3 user ทำไมมันทำได้เหมือนกันหมดเลยอ่ะ — admin ควรกำหนด role ได้หรือไม่"

**Result**: Accepted — Multiple changes:
1. Added `getUserFromToken()` helper in `api.ts` to decode JWT client-side
2. Updated `patients/page.tsx` and `appointments/page.tsx` to hide create forms for Viewer role
3. Created `NavBar.tsx` with role badge display and conditional Users menu link for Admin
4. Created `users/page.tsx` — Admin-only user management page (create user, change roles)
5. Added `username` claim to JWT in backend `AuthEndpoints.cs`

### What was iterated:
- **Navbar hydration issue** ("ทำไมต้องกดก่อนแล้ว menu ถึงจะขึ้นมาแสดง"): `useEffect` runs after SSR, so `user` is null on first render. Fixed by adding `mounted` state flag that shows "Loading..." placeholder until client hydration completes.
- **Users menu not appearing after login** ("login เข้ามาแบบ admin แล้วเมนู users ต้องขึ้นเลยสิ"): `router.push()` does client-side navigation — NavBar's `useEffect([])` doesn't re-run. Fixed by changing to `window.location.href` for full page reload after login.

## Prompt 7: Appointment Detail Modal
**Prompt**: "list การนัดหมาย เพิ่มให้อีกหน่อยสิ มีปุ่มให้กดดู detail ด้วย"

**Result**: Accepted — Added Detail button to each appointment row with a modal popup showing: Patient name, Phone, Branch, Start At, Created At, and Appointment ID. Modal closes on backdrop click or Close button.

## Prompt 8: README Review for Submission
**Prompt**: "ตรวจสอบ readme นี้ให้หน่อยสิ ว่าถ้าทำตามนี้แล้วสามารถ run program ได้ที่เครื่อง local ของผู้สอบได้เลยใช่ไหม?"

**Result**: Found 2 issues and fixed:
1. **Port mismatch**: `docker-compose.yml` had `3001:3000` (local workaround for Grafana conflict) but README said `localhost:3000`. Reverted to `3000:3000`.
2. **Missing setup info**: Added Prerequisites section (Docker Desktop, Git, available ports), git clone URL, auto-migration note, and Docker-based test option for evaluators without .NET SDK.

## Key Insight
The most valuable AI contribution was the **EF Core Global Query Filter pattern** for tenant isolation — it provides automatic, forgettable-proof data isolation. The most important human judgment was **recognizing the InMemory DB naming bug** in tests and the need to handle non-relational providers gracefully in the migration step. The iterative UI fixes (hydration, navigation, role enforcement) highlighted that **backend correctness alone is insufficient** — frontend must mirror authorization logic for a complete user experience.
