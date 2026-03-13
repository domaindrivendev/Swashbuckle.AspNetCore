# Coding Agent Instructions

This file provides guidance to agents when working with code in this repository.

## Overview

Swashbuckle.AspNetCore is an OpenAPI (Swagger) tooling library for ASP.NET Core. It generates OpenAPI 2.0/3.0/3.1 documents
from ASP.NET Core routes/controllers/models at runtime, and serves the swagger-ui and ReDoc UIs as embedded middleware.

It targets the currently released and supported versions of .NET.

## Commands

**Build and test (full pipeline):**

```powershell
./build.ps1
```

This runs `dotnet pack` followed by `dotnet test`.

**Build only:**

```bash
dotnet build
```

**Run all tests:**

```bash
dotnet test
```

**Run tests for a specific project:**

```bash
dotnet test test/Swashbuckle.AspNetCore.SwaggerGen.Test/
dotnet test test/Swashbuckle.AspNetCore.IntegrationTests/
```

**Run a single test:**

```bash
dotnet test test/Swashbuckle.AspNetCore.SwaggerGen.Test/ --filter "FullyQualifiedName~MethodName"
```

**Restore .NET tools:**

```bash
dotnet tool restore
```

**Update snapshot files (Verify tests):**

When snapshot-based tests fail due to intentional changes, update snapshots by running tests with the
`VERIFY_AUTO_ACCEPT_CHANGES=true` environment variable or by accepting the diff via the Verify framework's mechanism.

## Architecture

### Source packages (`src/`)

| Project | Purpose |
| --- | --- |
| `Swashbuckle.AspNetCore.Swagger` | Core middleware that serves OpenAPI JSON endpoints; depends on `Microsoft.OpenApi` |
| `Swashbuckle.AspNetCore.SwaggerGen` | OpenAPI document generation engine — the main logic layer |
| `Swashbuckle.AspNetCore.SwaggerUI` | Embeds swagger-ui as ASP.NET Core middleware (bundles from npm) |
| `Swashbuckle.AspNetCore.ReDoc` | Embeds ReDoc as ASP.NET Core middleware (bundles from npm) |
| `Swashbuckle.AspNetCore.Annotations` | Optional attributes (`SwaggerOperation`, `SwaggerResponse`, etc.) |
| `Swashbuckle.AspNetCore.Newtonsoft` | Newtonsoft.Json support replacing the default System.Text.Json contract resolver |
| `Swashbuckle.AspNetCore.Cli` | `dotnet swagger` CLI tool for offline document generation |
| `Swashbuckle.AspNetCore.ApiTesting` | Assertion helpers for API contract testing |
| `Swashbuckle.AspNetCore` | Meta-package referencing the above |

### Key generation pipeline (`SwaggerGen`)

1. **`SwaggerGenerator`** — entry point; uses `IApiDescriptionGroupCollectionProvider` (ASP.NET Core's API explorer) to enumerate endpoints, then calls `SchemaGenerator` for each type.
2. **`SchemaGenerator`** — converts .NET types to `IOpenApiSchema` objects. Uses `ISerializerDataContractResolver` to understand how the serializer will represent types.
3. **`JsonSerializerDataContractResolver`** — the default resolver backed by System.Text.Json; `NewtonsoftDataContractResolver` (in the Newtonsoft package) is the alternative.
4. **`SchemaRepository`** — accumulates reusable schemas into `components/schemas` to avoid duplication. Schemas are referenced by `$ref` after first generation.
5. **Filters** — `IDocumentFilter`, `IOperationFilter`, `IParameterFilter`, `IRequestBodyFilter`, `ISchemaFilter` (sync and async variants) are applied at each pipeline stage and are the primary extension points.

### Test structure (`test/`)

- `Swashbuckle.AspNetCore.SwaggerGen.Test` — unit tests for schema and swagger generation; uses xUnit v3 and Verify for snapshot testing; snapshots stored under `snapshots/{tfm}/`.
- `Swashbuckle.AspNetCore.IntegrationTests` — end-to-end tests running real `WebApplication` instances; also uses Verify snapshots and Playwright for UI tests.
- `WebSites/` — sample ASP.NET Core apps used as test fixtures (Basic, MinimalApp, MultipleVersions, OAuth2Integration, etc.).
- `Swashbuckle.AspNetCore.TestSupport` — shared test helpers.

### Public API tracking

Packable projects use `Microsoft.CodeAnalysis.PublicApiAnalyzers`. Any change to the public API surface must be reflected in `PublicAPI/PublicAPI.Unshipped.txt` (and per-TFM variants). Breaking changes require a baseline comparison against `PackageValidationBaselineVersion`.

### Warnings and strictness

`TreatWarningsAsErrors` is enabled globally. `Nullable` annotations are not yet enabled.

## Key conventions

- Central package version management via `Directory.Packages.props`.
- The primary serializer integration is System.Text.Json; Newtonsoft.Json is opt-in via the separate package.
- Integration test snapshots under `test/Swashbuckle.AspNetCore.IntegrationTests/snapshots/` are linted with Spectral in CI to validate OpenAPI correctness.
- SwaggerUI and ReDoc bundles are managed via npm (`package.json` in their respective `src/` directories).

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not change the public API unless specifically requested.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
