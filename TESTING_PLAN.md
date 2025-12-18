# TESTING PLAN

## Project
DuPharma Pharmacy management web application (ASP.NET Core Razor Pages / MVC + EF Core)

## Test objectives
- Verify core functional correctness: authentication, CRUD for users/medicines/batches/customers, sales/dispense, orders, prescriptions, contact messages, and reporting.
- Ensure security and access control via roles/permissions and API authentication (JWT).
- Validate data integrity (stock levels, cascades, totals), concurrency safety for concurrent sales, and expiry/low-stock logic.
- Confirm deployment and runtime behavior under expected environment (Docker + SQL Server).

## Test scope
- In scope: UI flows (Razor Pages), MVC controllers used by staff, REST API endpoints under `Controllers/Api/`, EF Core migrations and DB behavior, authentication/authorization, file uploads (prescriptions), CSV export.
- Out of scope: external integrations not present (third-party payment processors), load/performance beyond functional acceptance, and UI visual pixel-level tests.

## Test environment
- ASP.NET Core 8.0 runtime (project target), Kestrel behind HTTPS in Docker or local IIS Express
- SQL Server (local or Docker image) with schema from EF Core migrations
- Browser: Chrome (latest), Firefox (latest), Edge (latest)
- Tools: Postman for API tests, dotnet CLI for running app and migrations, Docker for containerized runs
- Sample test data: seed dataset (2 branches, 5 medicines, several batches with mixed expiry/stock), Admin/Manager/Pharmacist test accounts

## Entry criteria
- Application builds successfully (`dotnet build`) and runs locally or in Docker without startup errors.
- Database initialized and seeded; migrations applied.
- Test accounts created (Admin, Manager, Pharmacist).
- Swagger/API explorer accessible for API tests (development mode).

## Exit criteria
- All P0/P1 functional test cases pass.
- No outstanding security-critical findings (auth bypass, plaintext secrets).
- Regression tests for previously failing items pass.
- Test report produced with pass/fail results and logged defects.

## Test types
- Manual functional tests (UI flows)
- API functional tests (Postman)
- Integration tests (EF Core + local DB; optional automated)
- Security checks (auth, permission enforcement, password handling)
- Concurrency/consistency checks (competing sales/orders)
- File upload and download checks (prescription upload, CSV export)

## Test cases (manual & functional)

| ID | Title | Preconditions | Steps | Expected result | Type | Priority |
|----|-------|---------------|-------|-----------------|------|----------|
| TC-01 | Login with valid credentials | Admin user exists and is active | 1. Open login page 2. Enter admin email/password 3. Submit | User signs in, redirected to dashboard; auth cookie set and JWT token issued | Manual | P0 |
| TC-02 | Login with invalid credentials | none | Attempt login with wrong password | Login rejected, error shown | Manual | P0 |
| TC-03 | Create user (Admin only) | Logged in as Admin | Navigate to `Users/Create`, fill form, submit | New user persisted, password stored hashed, redirect to list | Manual | P1 |
| TC-04 | Create medicine | Authenticated user with `CreateMedicines` permission | `Medicines/Create` → submit valid medicine | Medicine saved, visible on `Medicines/Index` | Manual | P1 |
| TC-05 | Add batch and stock assigned to branch | Medicine exists; user has branch context | `Batches/Create` → set Branch, QuantityOnHand, ExpiryDate | Batch saved; `TotalStock` updates on medicine list | Manual | P1 |
| TC-06 | Create sale (normal flow) | Pharmacist user; stock present | Sales/Create → select items and quantities → submit | Sale created, InvoiceNumber generated, SaleItems persisted, Batch.QuantityOnHand reduced | Manual / Integration | P0 |
| TC-07 | Create sale with insufficient stock | Pharmacist user | Attempt sale where requested quantity > available stock | Dispense fails or throws; transaction rolled back; user shown error | Integration | P0 |
| TC-08 | API: Get medicines | App running | GET `/api/Medicines` with query | Returns medicines JSON with `TotalStock` and `LowestPrice` fields | API | P1 |
| TC-09 | API: Auth-required endpoint rejects anonymous | App running | GET `/api/Batches` without JWT | 401 Unauthorized | API | P0 |
| TC-10 | Upload prescription image | Orders flow available | POST file to `Orders/UploadPrescription` | File stored under `wwwroot/prescriptions`; JSON response with URL | Manual | P1 |
| TC-11 | Order to sale conversion | Order exists; Admin approves | Update order status to `Completed` | `DispenseService` creates sale; order notes updated; sale linked | Manual / Integration | P0 |
| TC-12 | Expiry alert calculation | Batches with expiry in 60 days exist | Call `/api/Batches/expiring?days=90` or view dashboard | Returned list includes qualifying batches | API/UI | P2 |
| TC-13 | Permissions enforcement | User lacking permission | Attempt to access page with `[RequirePermission]` | Access denied or HTTP 403 | Manual | P0 |
| TC-14 | Reports CSV export | Admin user | `Reports/ExportCsv?period=monthly` | CSV file download with expected headers and rows | Manual | P1 |
| TC-15 | Concurrent sales reduce stock atomically | Two simultaneous sale requests for same stock | Issue two CreateSale requests for overlapping quantities | Stock decremented correctly; one request fails if insufficient stock; no negative stock | Integration/Concurrency | P0 |
| TC-16 | Change password and verify hash | Admin updates user password | Update password in `Users/Edit` | Stored `PasswordHash` matches hashing algorithm; old password fails | Manual | P1 |
| TC-17 | Contact message lifecycle | Public user submits contact | Submit `Shop/Contact`, reply as staff | Contact message created; reply sends email via `IEmailService`; message marked replied | Manual | P2 |
| TC-18 | API: Sales summary accuracy | Known sales dataset present | GET `/api/Sales/summary` with date range | Aggregate numbers match DB totals (count, revenue, averages) | API | P1 |

## Test data management
- Use a seed script or EF Core seeding to create deterministic data for tests.
- For concurrency tests, use an isolated DB instance to avoid interference with manual testing.

## Test execution notes
- Run acceptance tests on a clean database instance.
- For API tests use Postman collections that assert status codes and JSON schemas.
- For critical flows (sales/dispense) include DB assertions (stock before/after) and confirm rollback on failure.

## Defect reporting & severity
- P0: Authentication bypass, data loss, negative stock, sale creation failures.
- P1: Business logic errors (wrong totals), missing permission enforcement, missing pages.
- P2: UI/UX issues, non-critical export problems.
