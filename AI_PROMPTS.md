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

## Key Insight
The most valuable AI contribution was the **EF Core Global Query Filter pattern** for tenant isolation — it provides automatic, forgettable-proof data isolation. The most important human judgment was **recognizing the InMemory DB naming bug** in tests and the need to handle non-relational providers gracefully in the migration step.
