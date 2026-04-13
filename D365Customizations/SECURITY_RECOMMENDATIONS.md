# Security recommendations

## Implemented in code

- **No secrets in source**: `App.config` ships with empty `CrmConnection`; use environment variables (`CRM_CONNECTION_STRING`, `ORDER_API_URL`) or deployment-time transforms.
- **HTTPS**: `ExternalApiClient` should target `https://` endpoints in production.
- **Least privilege**: create an **Application User** in Dataverse with only **Read** on `salesorder` (and related tables if you resolve customer names) for the integration identity.

## Recommended for production

1. **Dataverse auth**: OAuth 2.0 **client credentials** with Azure AD app registration; store client secret in **Azure Key Vault** or pipeline secrets.
2. **External API**: API key or OAuth in an HTTP header; store keys in Key Vault; rotate regularly.
3. **Webhooks**: protect Azure Function with **function keys** or **App Service authentication**; validate issuer and replay where applicable.
4. **Transport**: TLS 1.2+ only; disable legacy protocols.
5. **Resilience**: consider **Polly** for retries on transient HTTP failures (optional extension).

## Order mock (Azure Function)

`OrderMock.Functions` is a **placeholder HTTP trigger** for development and demos. In Azure, protect it with **Function keys** (`AuthorizationLevel.Function`), **Easy Auth**, or **API Management**; avoid anonymous endpoints on the public internet.
