## Purpose

Provides authentication for the API: validates credentials via `POST /login` and issues a signed JWT that downstream endpoints use to authorize requests. This change migrates the authentication components into the layered architecture without altering the login contract.

## Requirements

### Requirement: Login with valid credentials
The system SHALL accept a `POST /login` request with an email and password, and return HTTP 200 with a JSON body containing a `token` field when the credentials match the configured user (`admin` / `1234`).

#### Scenario: Valid credentials
- **WHEN** a client calls `POST /login` with email `admin` and password `1234`
- **THEN** the system returns HTTP 200 with a JSON body containing a `token` field

#### Scenario: Invalid credentials
- **WHEN** a client calls `POST /login` with any other email or password
- **THEN** the system returns HTTP 401 without a token

### Requirement: Token carries identity claims
The system SHALL sign the returned JWT with the configured secret using HMAC-SHA256, set the configured issuer and audience, set an expiration of one hour, and include the user's id, email, and role as claims.

#### Scenario: Decode the issued token
- **WHEN** a client decodes a token returned by `POST /login`
- **THEN** the token contains the user id, email `admin`, and role `admin` as claims, matches the configured issuer and audience, expires approximately one hour after issuance, and verifies against the configured secret
