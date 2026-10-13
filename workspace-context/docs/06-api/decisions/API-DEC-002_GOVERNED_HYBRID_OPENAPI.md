# API-DEC-002 — Governed Hybrid OpenAPI

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026  
> **Selected option:** C — Governed Hybrid OpenAPI

## Decision

- Typed/versioned C# transport and application contracts are the implementation source.
- OpenAPI is generated automatically from the API contract.
- Normalized OpenAPI is stored and versioned as a contract artifact per API major.
- CI performs OpenAPI validation, snapshot comparison, and breaking-change diff.
- Contract tests are mandatory for request, response, serialization, validation, and registered server handlers.
- Manual API documentation is not a source of truth.

## Guard

- Stable explicit `operationId`.
- Named components for shared envelopes and typed server-executable Feature ID schemas.
- Local-only features are not exposed as HTTP processing operations.
- Engine/provider classes do not become transport contracts.
- Breaking change requires a new major or an approved compatibility/migration path.
- Examples and artifacts contain no token, password, secret, document content, full OCR text, signature asset, local path, or Production data.

## Traceability

`IR-API-001..012`; API/version compatibility requirements; `API-DEC-001`; Solution Design REST, compatibility, Assistant, and architecture-test boundaries; Technical Reference testing and CI security gates.
