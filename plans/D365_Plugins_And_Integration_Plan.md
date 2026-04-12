# D365 CRM Plugins and Integration -- Development Plan

## Overview

Build a single .NET Framework 4.6.2 solution (`D365.Customizations.sln`) containing two D365 CRM plugins, a D365-to-external-API integration console application, and a unified xUnit + Moq test project covering everything. All code follows TDD and is designed around SOLID principles with proven design patterns for reusability and scalability.

---

## 1. SOLID Principles -- How They Map to This Solution

### S -- Single Responsibility Principle

Every class has exactly one reason to change:
- **PluginBase** only handles SDK service extraction (pipeline plumbing)
- **Plugin classes** only act as composition roots -- they wire dependencies and delegate
- **Service classes** only contain business logic (validation, creation, sync)
- **Constants classes** only hold entity metadata (logical names, messages)
- **Repository classes** only handle D365 data access queries
- **ApiClient classes** only handle HTTP communication

### O -- Open/Closed Principle

- `PluginBase` is open for extension (new plugins inherit it) but closed for modification (extraction logic never changes)
- New entity plugins are added by creating new service classes -- no existing code is modified
- `IExternalApiClient` can have new implementations (real, mock, retry-wrapper) without changing `OrderSyncService`

### L -- Liskov Substitution Principle

- `MockExternalApiClient` and `ExternalApiClient` are fully interchangeable through `IExternalApiClient`
- Any `PluginBase` subclass can be registered in D365 without the pipeline caring which one it is

### I -- Interface Segregation Principle

- `IDuplicateContactValidator` has one method: `bool EmailExists(string email)`
- `IChildContactService` has one method: `Guid CreateChildContact(Guid accountId, string accountName)`
- `IExternalApiClient` has one method: `ApiResponse SendOrderAsync(OrderDetailsPayload payload)`
- `IOrderRepository` has one method: `List<Entity> GetOrdersCreatedSince(DateTime since)`
- No client is forced to depend on methods it does not use

### D -- Dependency Inversion Principle

- Plugin business logic depends on `IDuplicateContactValidator` / `IChildContactService` (abstractions), never on concrete query implementations
- `OrderSyncService` depends on `IOrderRepository` + `IExternalApiClient` (abstractions), never on `IOrganizationService` or `HttpClient` directly
- All concrete dependencies are injected via constructors -- nothing is `new`-ed inside business logic

---

## 2. Design Patterns Used

### Template Method Pattern (PluginBase)

The most critical pattern for D365 plugin scalability. `PluginBase` is an abstract class that handles the repetitive SDK extraction boilerplate. Subclasses override a single method with their business logic:

```
PluginBase (abstract, implements IPlugin)
  │
  │  Execute(IServiceProvider serviceProvider)
  │    → Extract IPluginExecutionContext
  │    → Extract IOrganizationServiceFactory → IOrganizationService
  │    → Extract ITracingService
  │    → Validate Target exists and is Entity
  │    → Call abstract ExecuteBusinessLogic(localContext)
  │
  └── LocalPluginContext (inner class)
        Bundles: Context, Service, TracingService, Target
        Passed to ExecuteBusinessLogic() as a single object
```

Every future plugin inherits `PluginBase` and only writes business logic. Zero boilerplate duplication across 2, 20, or 200 plugins.

### Strategy Pattern (API Clients)

`IExternalApiClient` defines the contract. Multiple strategies implement it:
- `ExternalApiClient` -- real HTTP calls for production
- `MockExternalApiClient` -- in-memory responses for local dev/demo

`OrderSyncService` accepts any strategy via constructor and is completely unaware of which one it uses.

### Repository Pattern (Data Access Abstraction)

`IOrderRepository` abstracts the D365 query mechanics:
- `OrderRepository` uses `IOrganizationService` + `QueryExpression` internally
- Tests mock `IOrderRepository` directly -- no need to mock the entire Xrm SDK query chain
- If D365 is replaced by another CRM, only the repository implementation changes

### Composition Root Pattern (Plugin Classes)

Each plugin class is a **composition root** -- the only place where concrete types are instantiated. Business logic never creates its own dependencies:

```
PreventDuplicateContactByEmail : PluginBase
  └── ExecuteBusinessLogic(localContext)
        var validator = new DuplicateContactValidator(localContext.Service);
        validator.ValidateNoDuplicateEmail(target);
```

This is the standard D365 pattern because the plugin framework instantiates plugins -- we cannot use a DI container. The composition root is the next best thing.

---

## 3. Design Decisions

### Testing Approach: xUnit + Moq (over FakeXrmEasy)

- More versatile -- full control over mock behavior for every D365 SDK interface
- Tests explicitly document which service calls each plugin makes
- Mirrors the existing `xrm-mock-factory.js` pattern -- `PluginMockFactory` wires the `IServiceProvider` chain
- With service extraction (SOLID), most tests mock just `IOrganizationService` directly -- simpler and faster than full pipeline mocks
- FakeXrmEasy abstracts too much, hiding SDK internals behind convenience methods

### Two-Level Testing Strategy

```
                     Plugin Tests (integration-level)
                     ┌─────────────────────────────────┐
                     │ Mock IServiceProvider chain      │
                     │ Test: extraction + delegation    │
                     │ Test: defensive edge cases       │
                     └────────────┬────────────────────┘
                                  │ delegates to
                     Service Tests (unit-level)
                     ┌─────────────────────────────────┐
                     │ Mock only IOrganizationService   │
                     │ Test: pure business logic        │
                     │ Test: query correctness          │
                     │ Test: field mappings             │
                     └─────────────────────────────────┘
```

- **Service tests** mock only what the service needs (IOrganizationService) -- fast, focused, test business logic in isolation
- **Plugin tests** mock the full IServiceProvider chain -- verify extraction + delegation + edge cases
- This follows the testing pyramid: many focused unit tests, fewer integration-level tests

### Target Framework: .NET Framework 4.6.2

- Required for D365 online plugin deployment (widest compatibility)
- Build on Windows via Visual Studio
- Console application also targets net462 for SDK consistency

### Late-Bound Entities (over Early-Bound)

- No code generation step required -- more portable and self-contained
- Uses `Entity` class with string-based attribute access
- Constants classes hold all logical names to avoid hardcoded strings throughout

---

## 4. Target Framework and NuGet Packages

### Plugin Projects (`D365.Plugins.Contact`, `D365.Plugins.Account`)

- `Microsoft.CrmSdk.CoreAssemblies` (9.0.2.x)

### Common Project (`D365.Plugins.Common`)

- `Microsoft.CrmSdk.CoreAssemblies` (9.0.2.x)

### Integration Project (`D365.Integration.OrderSync`)

- `Microsoft.CrmSdk.CoreAssemblies` (9.0.2.x)
- `Newtonsoft.Json` (13.x) -- JSON serialization for API payloads

### Test Project (`D365.Customizations.Tests`)

- `Microsoft.CrmSdk.CoreAssemblies` (9.0.2.x)
- `xunit` (2.9.x)
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`
- `Moq` (4.20.x)

---

## 5. Solution Structure

```
Plugins/
  D365.Customizations.sln
  │
  ├── D365.Plugins.Common/                              -- Shared constants, base classes, interfaces
  │     D365.Plugins.Common.csproj
  │     Constants/
  │       ContactConstants.cs                            -- Contact logical names, error messages
  │       AccountConstants.cs                            -- Account logical names, trace messages
  │       OrderConstants.cs                              -- SalesOrder logical names, API config
  │     Base/
  │       PluginBase.cs                                  -- Template Method: extracts services, calls abstract method
  │       LocalPluginContext.cs                          -- Bundles Context + Service + TracingService + Target
  │
  ├── D365.Plugins.Contact/                              -- Contact entity plugins + services
  │     D365.Plugins.Contact.csproj                      -- References D365.Plugins.Common
  │     PreventDuplicateContactByEmail.cs                -- Plugin (composition root, inherits PluginBase)
  │     Services/
  │       IDuplicateContactValidator.cs                  -- Interface: bool EmailExists(string email)
  │       DuplicateContactValidator.cs                   -- Implementation: QueryExpression on contact
  │
  ├── D365.Plugins.Account/                              -- Account entity plugins + services
  │     D365.Plugins.Account.csproj                      -- References D365.Plugins.Common
  │     CreateChildContactOnAccountCreate.cs             -- Plugin (composition root, inherits PluginBase)
  │     Services/
  │       IChildContactService.cs                        -- Interface: Guid CreateChildContact(...)
  │       ChildContactService.cs                         -- Implementation: service.Create(contact)
  │
  ├── D365.Integration.OrderSync/                        -- Console app: Order → External API
  │     D365.Integration.OrderSync.csproj                -- References D365.Plugins.Common
  │     Program.cs                                       -- Entry point (composition root for the console app)
  │     Services/
  │       IExternalApiClient.cs                          -- Interface: ApiResponse SendOrderAsync(payload)
  │       ExternalApiClient.cs                           -- Real HttpClient implementation
  │       MockExternalApiClient.cs                       -- Mock implementation for local testing
  │       IOrderRepository.cs                            -- Interface: List<Entity> GetOrdersCreatedSince(DateTime)
  │       OrderRepository.cs                             -- Implementation using IOrganizationService
  │       OrderSyncService.cs                            -- Orchestrator: IOrderRepository + IExternalApiClient
  │     Models/
  │       OrderDetailsPayload.cs                         -- DTO: CustomerName, OrderTotal, OrderDate
  │       ApiResponse.cs                                 -- DTO: IsSuccess, StatusCode, ErrorMessage
  │
  └── D365.Customizations.Tests/                         -- Single test project covering ALL above
        D365.Customizations.Tests.csproj                 -- References all 4 projects
        Helpers/
          PluginMockFactory.cs                           -- Builds full IServiceProvider mock chain
        Contact/
          DuplicateContactValidatorTests.cs              -- Service-level unit tests (mock IOrganizationService)
          PreventDuplicateContactByEmailTests.cs         -- Plugin-level tests (mock IServiceProvider chain)
        Account/
          ChildContactServiceTests.cs                    -- Service-level unit tests
          CreateChildContactOnAccountCreateTests.cs      -- Plugin-level tests
        Integration/
          OrderSyncServiceTests.cs                       -- Mock IOrderRepository + IExternalApiClient
          ExternalApiClientTests.cs                      -- Mock HttpMessageHandler
```

---

## 6. Plugin 1: PreventDuplicateContactByEmail

### Registration

- **Entity**: `contact`
- **Message**: `Create`
- **Stage**: PreValidation (Stage 10) -- runs BEFORE the database transaction, blocking is cheap
- **Execution Mode**: Synchronous

### Class Responsibilities

```
PreventDuplicateContactByEmail : PluginBase       [Composition Root]
  │  Overrides ExecuteBusinessLogic()
  │  Creates DuplicateContactValidator(service)
  │  Extracts email from Target
  │  Calls validator.ValidateNoDuplicateEmail(email)
  │
  └── DuplicateContactValidator : IDuplicateContactValidator    [Business Logic]
        Constructor: IOrganizationService
        EmailExists(email):
          → QueryExpression: contact, emailaddress1 = email, TopCount = 1, ColumnSet(contactid)
          → return results.Entities.Count > 0
        ValidateNoDuplicateEmail(email):
          → if null/empty/whitespace → return (skip check)
          → if EmailExists(email) → throw InvalidPluginExecutionException
```

### Scalability Considerations

- `TopCount = 1` stops scanning after one match
- Only retrieves `contactid` column -- minimal data transfer
- PreValidation stage means no database transaction overhead on rejection
- Service class can be reused from custom APIs, workflows, or other plugins

---

## 7. Plugin 2: CreateChildContactOnAccountCreate

### Registration

- **Entity**: `account`
- **Message**: `Create`
- **Stage**: PostOperation (Stage 40) -- Account must exist before child Contact can reference it
- **Execution Mode**: Synchronous

### Class Responsibilities

```
CreateChildContactOnAccountCreate : PluginBase     [Composition Root]
  │  Overrides ExecuteBusinessLogic()
  │  Creates ChildContactService(service, tracingService)
  │  Extracts accountId + accountName from Target
  │  Calls childContactService.CreateChildContact(accountId, accountName)
  │
  └── ChildContactService : IChildContactService              [Business Logic]
        Constructor: IOrganizationService, ITracingService
        CreateChildContact(accountId, accountName):
          → Build Contact entity: firstname="Default", lastname=accountName, parentcustomerid=account ref
          → service.Create(contact)
          → tracingService.Trace("Child contact created for account: {accountName}")
          → return contactId
```

---

## 8. Integration: OrderSync Console Application

### Architecture

```
Program.cs (Composition Root)
  │  Creates CrmServiceClient (D365 connection)
  │  Creates OrderRepository(service)
  │  Creates ExternalApiClient(httpClient, apiUrl)
  │  Creates OrderSyncService(repository, apiClient)
  │  Runs polling loop
  │
  ├── OrderSyncService                                 [Orchestrator]
  │     Constructor: IOrderRepository + IExternalApiClient
  │     SyncNewOrders(since):
  │       → repository.GetOrdersCreatedSince(since)
  │       → For each order: build payload → apiClient.SendOrderAsync(payload) → log result
  │
  ├── OrderRepository : IOrderRepository               [Data Access]
  │     Constructor: IOrganizationService
  │     GetOrdersCreatedSince(since):
  │       → QueryExpression on salesorder where createdon >= since
  │       → Returns List<Entity>
  │
  └── ExternalApiClient : IExternalApiClient           [HTTP Communication]
        Constructor: HttpClient + apiUrl
        SendOrderAsync(payload):
          → Serialize payload to JSON
          → POST to apiUrl
          → Return ApiResponse with status
```

### SalesOrder Fields Used

- `name` → CustomerName (order name/description)
- `totalamount` → OrderTotal (monetary value)
- `createdon` → OrderDate (creation timestamp)
- `customerid` → CustomerId (EntityReference to customer)

### Trigger Mechanisms (Documentation)

The `INTEGRATION_GUIDE.md` will describe two approaches:

1. **Webhook (Real-Time)**: Register a Service Endpoint in Plugin Registration Tool pointing to an Azure Function URL. Register a step on `salesorder` Create PostOperation Async. D365 POSTs the `RemoteExecutionContext` to the endpoint on every order creation.

2. **Polling (Batch)**: Console app runs on a schedule (Windows Task Scheduler / Azure WebJob). Queries D365 for orders created since last poll. Processes them in batch.

### Security Recommendations (Documentation)

The `SECURITY_RECOMMENDATIONS.md` will cover:

1. **D365 Connection Authentication** -- OAuth 2.0 Client Credentials flow via Azure AD App Registration. Never store secrets in code -- use `app.config` transforms or Azure Key Vault.
2. **External API Authentication** -- API Key in HTTP header or mutual TLS. Key stored in Azure Key Vault.
3. **Webhook Endpoint Security** -- `WebhookKey` authentication via Azure Function auth keys.
4. **Transport Security** -- HTTPS/TLS 1.2+ for all communications.
5. **Least Privilege** -- D365 App User scoped to minimum required entity permissions.
6. **Retry + Circuit Breaker** -- Polly library for transient fault handling on HTTP calls.

---

## 9. Shared Mock Factory (PluginMockFactory.cs)

Mirrors the `xrm-mock-factory.js` pattern. Returns a `PluginMockContext` object containing all mocks:

```
CreatePluginMockContext(Entity target, string messageName, int stage)
  Returns:
    - Mock<IServiceProvider> ServiceProvider
    - Mock<IOrganizationService> OrganizationService     ← tests configure returns + assert calls
    - Mock<ITracingService> TracingService                ← tests assert Trace messages
    - Mock<IPluginExecutionContext> PluginContext          ← pre-configured with Target, MessageName, Stage
```

For service-level tests (no pipeline), tests create `Mock<IOrganizationService>` directly -- no factory needed.

---

## 10. Test Cases (TDD -- Write First, Implement Second)

### Service Tests: DuplicateContactValidator (Unit Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-PV01 | EmailExists with existing contact | Returns true |
| TC-PV02 | EmailExists with no existing contact | Returns false |
| TC-PV03 | ValidateNoDuplicateEmail with unique email | No exception, RetrieveMultiple called |
| TC-PV04 | ValidateNoDuplicateEmail with duplicate email | Throws `InvalidPluginExecutionException` with exact message |
| TC-PV05 | ValidateNoDuplicateEmail with null email | No exception, RetrieveMultiple NOT called |
| TC-PV06 | ValidateNoDuplicateEmail with empty string | No exception, RetrieveMultiple NOT called |
| TC-PV07 | ValidateNoDuplicateEmail with whitespace | No exception, RetrieveMultiple NOT called |
| TC-PV08 | QueryExpression uses TopCount = 1 for performance | Verify captured QueryExpression |

### Plugin Tests: PreventDuplicateContactByEmail (Pipeline Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-P01 | Full pipeline: unique email | No exception |
| TC-P02 | Full pipeline: duplicate email | `InvalidPluginExecutionException` |
| TC-P03 | Null Target in InputParameters | No exception (PluginBase guard) |
| TC-P04 | Target is not an Entity type | No exception (PluginBase guard) |

### Service Tests: ChildContactService (Unit Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-AV01 | CreateChildContact calls service.Create | `service.Create()` called once |
| TC-AV02 | Created contact has correct fields | firstname="Default", lastname=accountName |
| TC-AV03 | Created contact has correct parent reference | parentcustomerid = EntityReference("account", accountId) |
| TC-AV04 | TracingService.Trace called with success message | Trace message contains account name |
| TC-AV05 | Account with null name | Child contact created with lastname = null, no crash |

### Plugin Tests: CreateChildContactOnAccountCreate (Pipeline Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-A01 | Full pipeline: account create | service.Create() called with Contact entity |
| TC-A02 | Full pipeline: trace log written | TracingService.Trace() called |
| TC-A03 | Null Target in InputParameters | No exception (PluginBase guard) |
| TC-A04 | Target is not an Entity type | No exception (PluginBase guard) |

### Service Tests: OrderSyncService (Unit Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-I01 | Valid order, API returns success | Success logged, no exception |
| TC-I02 | Payload has correct field mapping | CustomerName, OrderTotal, OrderDate match entity values |
| TC-I03 | API returns failure response | Failure logged with error details |
| TC-I04 | API throws exception | Failure logged, exception handled gracefully |
| TC-I05 | Order with null customer name | Payload sent with null CustomerName, no crash |
| TC-I06 | Multiple orders in batch | All processed, each logged individually |
| TC-I07 | No orders found | No API calls made, no errors |

### Service Tests: ExternalApiClient (Unit Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-I08 | SendOrderAsync posts correct JSON payload | HttpClient.PostAsync called with correct URL and body |
| TC-I09 | HTTP 200 response | Returns ApiResponse.IsSuccess = true |
| TC-I10 | HTTP 500 response | Returns ApiResponse with IsSuccess = false and StatusCode |
| TC-I11 | Network failure (HttpRequestException) | Returns ApiResponse with IsSuccess = false and ErrorMessage |

### PluginBase Tests (Framework Level)

| ID | Scenario | Expected |
|-|-|-|
| TC-B01 | Valid IServiceProvider extracts all services correctly | ExecuteBusinessLogic called with populated LocalPluginContext |
| TC-B02 | Null Target in InputParameters | ExecuteBusinessLogic NOT called, no exception |
| TC-B03 | InputParameters missing "Target" key | ExecuteBusinessLogic NOT called, no exception |

**Total: 34 test cases across 7 test classes**

---

## 11. Comment and Code Style (C# Translation of Your JS Conventions)

| JS Pattern | C# Equivalent |
|-|-|
| `//!` section emphasis | `//!` (identical) |
| `//*` explanatory comment | `//*` (identical) |
| `/** ** ... */` file banner | `/** ** ... */` (identical) |
| `#region` / `#endregion` | `#region` / `#endregion` (native C#, no `//` prefix) |
| JSDoc `@param` / `@returns` | `/// <summary>` / `/// <param>` / `/// <returns>` XML docs |
| Test IDs `TC-C01`, `TC-O01` | `TC-PV01` (Validator), `TC-P01` (Plugin), `TC-AV01` (Account Validator), `TC-A01` (Account Plugin), `TC-I01` (Integration), `TC-B01` (Base) |
| File banner with Created On / Author | Same pattern in every .cs file |

---

## 12. Plugin Registration Steps (PLUGIN_REGISTRATION.md)

Step-by-step instructions for:

1. **Install Plugin Registration Tool** via `pac tool prt`
2. **Register New Assembly** -- Sandbox isolation mode, Database storage
3. **Register Steps**:
   - `PreventDuplicateContactByEmail`: Message=Create, Entity=contact, Stage=PreValidation, Mode=Synchronous
   - `CreateChildContactOnAccountCreate`: Message=Create, Entity=account, Stage=PostOperation, Mode=Synchronous

---

## 13. CI/CD Pipeline Skeleton (azure-pipelines.yml)

Two-stage Azure DevOps YAML pipeline:

**Build Stage**: NuGet restore → MSBuild → Run xUnit tests → Publish artifacts

**Deploy Stage** (skeleton): Download artifacts → `pac plugin push` or spkl → Deploy console app to WebJob

---

## 14. Documentation Deliverables

| File | Contents |
|-|-|
| `PLUGIN_REGISTRATION.md` | Step-by-step PRT instructions |
| `INTEGRATION_GUIDE.md` | Webhook vs Polling trigger setup, connection config |
| `SECURITY_RECOMMENDATIONS.md` | OAuth, Key Vault, WebhookKey, TLS, least privilege |
| `azure-pipelines.yml` | Skeleton CI/CD pipeline YAML |

---

## 15. Implementation Order (TDD Flow)

1. Scaffold solution structure + .csproj files + NuGet packages
2. Create `D365.Plugins.Common` -- constants, `PluginBase`, `LocalPluginContext`
3. Create `PluginMockFactory.cs` in test project
4. **TDD: PluginBase** -- Write TC-B01 to TC-B03 → Implement PluginBase → Verify
5. **TDD: Contact Validator** -- Write TC-PV01 to TC-PV08 (RED) → Implement `DuplicateContactValidator` (GREEN)
6. **TDD: Contact Plugin** -- Write TC-P01 to TC-P04 (RED) → Implement `PreventDuplicateContactByEmail` (GREEN)
7. **TDD: Account Service** -- Write TC-AV01 to TC-AV05 (RED) → Implement `ChildContactService` (GREEN)
8. **TDD: Account Plugin** -- Write TC-A01 to TC-A04 (RED) → Implement `CreateChildContactOnAccountCreate` (GREEN)
9. **TDD: Integration Services** -- Write TC-I01 to TC-I11 (RED) → Implement `OrderSyncService` + `ExternalApiClient` + `OrderRepository` (GREEN)
10. Create `Program.cs` (console app entry point)
11. Create documentation files
12. Create `azure-pipelines.yml` skeleton
13. Final test run + verify all 34 tests pass
