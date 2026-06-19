# ADR-001: Use TrueLayer as Open Banking Aggregator

**Status:** Accepted  
**Date:** June 2026  
**Deciders:** Abeer Farid  

## Context

FinTrack needs to connect to UK bank accounts (initially Bank of Scotland and Monzo)
to retrieve transaction history and account balances. Both banks expose UK Open Banking
APIs under PSD2 regulation, but each has different OAuth2 flows, data schemas, rate
limits, and error formats. Building two separate direct integrations would mean
maintaining duplicate auth logic, token management, and response mapping for every
bank added in future.

## Decision

Use TrueLayer as a single Open Banking aggregator. TrueLayer is an FCA-authorised
AISP (Account Information Service Provider) that handles the OAuth2 flow with each
bank, normalises the response schema, and returns a consistent API regardless of
which bank the user connects.

## Options Considered

**Option A — Direct bank API integration**  
Integrate separately with Bank of Scotland Open Banking API and Monzo API.  
- Pros: No third-party dependency, lower cost at scale  
- Cons: Two OAuth flows, different schemas, double the error handling,
  requires individual AISP registration per bank, significantly longer build time

**Option B — TrueLayer (chosen)**  
Single API covering 30+ UK banks. Free sandbox for development.  
- Pros: One integration for all banks, normalised data model, free sandbox,
  handles token refresh, FCA regulated AISP, .NET client library available  
- Cons: Third-party dependency, pricing at production scale,
  transaction data passes through TrueLayer servers

**Option C — Plaid UK**  
US-origin aggregator with UK coverage.  
- Cons: Less optimised for UK Open Banking, smaller UK bank coverage than TrueLayer,
  less transparent pricing

## Consequences

- Single integration point — adding Barclays, HSBC, or any other UK bank
  requires zero backend code changes
- TrueLayer sandbox enables full development and demo without real bank accounts
- TrueLayer access/refresh tokens must be stored encrypted at rest (Azure Key Vault)
- Users must re-authorise every 90 days (PSD2 regulatory requirement)
- Exit path: if TrueLayer pricing becomes unacceptable at scale, migrate to
  direct bank API integration per bank

## References

- [TrueLayer Data API docs](https://docs.truelayer.com)
- [UK Open Banking Standard](https://www.openbanking.org.uk)
- [PSD2 regulation overview](https://www.fca.org.uk/firms/payment-services-regulations)