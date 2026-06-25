# ADR-002: Store TrueLayer tokens encrypted in PostgreSQL

**Status:** Accepted  
**Date:** June 2026  
**Deciders:** Abeer Farid  

## Context

TrueLayer issues OAuth2 access tokens (short-lived, ~1 hour) and refresh 
tokens (long-lived, up to 90 days) after a user connects their bank. These 
tokens must be stored somewhere persistent so background sync jobs can 
fetch new transactions without requiring the user to re-authenticate on 
every request.

Two options were considered: encrypting tokens and storing them in the 
existing PostgreSQL database, or storing only a reference in the database 
and keeping the actual token values in Azure Key Vault.

## Decision

Store tokens encrypted with AES-256-CBC directly in the `bank_connections` 
table in PostgreSQL. The encryption key itself is stored in Azure Key Vault 
(production) or dotnet user-secrets (local development).

## Options Considered

**Option A — Encrypted storage in PostgreSQL (chosen)**  
Encrypt each token with AES-256-CBC and a random IV before inserting.  
- Pros: Simple, no additional Azure service dependency at runtime,
  fast reads (no Key Vault round-trip per sync job), encryption key
  is the only secret that needs protecting
- Cons: If the database AND the encryption key are both compromised
  simultaneously, tokens are exposed

**Option B — Azure Key Vault per-token storage**  
Store each token as a named secret in Key Vault, reference by name in DB.  
- Pros: Tokens never touch the database in plaintext, Key Vault provides 
  audit logs per-token access
- Cons: Every sync job requires a Key Vault API call, adding latency and
  a network dependency to background jobs. At scale (many users, frequent
  syncs) this becomes expensive. Key Vault has per-operation pricing.

## Consequences

- `access_token_encrypted` and `refresh_token_encrypted` columns on 
  `bank_connections` store AES-256-CBC ciphertext with prepended IV
- `AesTokenEncryptionService` handles encrypt/decrypt — tested independently
- Encryption key lives in user-secrets locally, Azure Key Vault in production
- If the encryption key is rotated, all stored tokens must be re-encrypted
  (acceptable — key rotation is an infrequent, planned operation)
- Token values are never logged — Serilog destructuring policy masks them