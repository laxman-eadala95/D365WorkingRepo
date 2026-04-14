# Plugin Registration (Plugin Registration Tool)

## Prerequisites

- Install the Plugin Registration Tool: `pac tool prt` (Power Platform CLI), or use the classic Plugin Registration Tool from NuGet/Visual Studio.
- Build **Release** configuration and locate `D365.SalesPlugins.dll` under `D365.SalesPlugins\bin\Release\net462\`.

## Register assembly

1. Connect to your Dataverse environment (OAuth recommended).
2. **Register** → **Register New Assembly**.
3. Select `D365.SalesPlugins.dll`.
4. Set **Isolation mode** to **Sandbox** (online requirement).
5. Set **Location** to **Database**.
6. Complete registration.

## Register steps

### PreventDuplicateContactByEmail

| Setting | Value |
|--------|--------|
| Message | Create |
| Primary Entity | contact |
| Eventing Pipeline Stage of Execution | **Pre-validation** (10) |
| Execution Mode | Synchronous |
| Type | Plugin |

Select the type `D365.SalesPlugins.PreventDuplicateContactByEmail`.

### CreateChildContactOnAccountCreate

| Setting | Value |
|--------|--------|
| Message | Create |
| Primary Entity | account |
| Eventing Pipeline Stage of Execution | **PostOperation** (40) |
| Execution Mode | Synchronous |
| Type | Plugin |

Select the type `D365.SalesPlugins.CreateChildContactOnAccountCreate`.

## Notes

- Pre-validation for duplicate email avoids starting a database transaction when the record is invalid.
- Post-operation on account ensures the account primary key exists before creating the child contact.
