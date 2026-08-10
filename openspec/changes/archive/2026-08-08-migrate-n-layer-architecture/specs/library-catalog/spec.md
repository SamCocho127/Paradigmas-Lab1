## Purpose

Manages the library catalog: libraries and the books contained in each library, exposed through REST endpoints under `/api/libraries`. Established by this capability are the URL scheme, status-code contract, and CRUD behavior the integration tests assert.

## ADDED Requirements

### Requirement: List libraries
The system SHALL return all libraries when `GET /api/libraries` is called.

#### Scenario: Libraries exist
- **WHEN** a client calls `GET /api/libraries`
- **THEN** the system returns HTTP 200 with a JSON array of libraries

#### Scenario: No libraries exist
- **WHEN** a client calls `GET /api/libraries` and there are no libraries
- **THEN** the system returns HTTP 200 with an empty JSON array

### Requirement: Get a library by id
The system SHALL return a single library for `GET /api/libraries/{libraryId}` when it exists, and HTTP 404 when it does not.

#### Scenario: Library exists
- **WHEN** a client calls `GET /api/libraries/{libraryId}` for an existing library
- **THEN** the system returns HTTP 200 with that library

#### Scenario: Library does not exist
- **WHEN** a client calls `GET /api/libraries/{libraryId}` for a library that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Create a library
The system SHALL create a library on `POST /api/libraries` and return the created library with HTTP 201.

#### Scenario: Valid library payload
- **WHEN** a client calls `POST /api/libraries` with a valid library name and location
- **THEN** the system creates the library, assigns it an id, and returns HTTP 201 with the created library

### Requirement: Update a library
The system SHALL update an existing library on `PUT /api/libraries/{libraryId}` and return HTTP 204, or HTTP 404 when the library does not exist.

#### Scenario: Library exists
- **WHEN** a client calls `PUT /api/libraries/{libraryId}` for an existing library with updated name/location
- **THEN** the system persists the update and returns HTTP 204

#### Scenario: Library does not exist
- **WHEN** a client calls `PUT /api/libraries/{libraryId}` for a library that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Delete a library
The system SHALL delete a library and all books it contains on `DELETE /api/libraries/{libraryId}` and return HTTP 204, or HTTP 404 when the library does not exist. After deletion the library and its books SHALL no longer be retrievable.

#### Scenario: Library exists with books
- **WHEN** a client calls `DELETE /api/libraries/{libraryId}` for a library that has books
- **THEN** the system deletes the library and its books and returns HTTP 204

#### Scenario: Library does not exist
- **WHEN** a client calls `DELETE /api/libraries/{libraryId}` for a library that does not exist
- **THEN** the system returns HTTP 404

#### Scenario: Books of a deleted library are gone
- **WHEN** a client calls `GET /api/libraries/{libraryId}/books` for a library deleted earlier
- **THEN** the system returns HTTP 404

### Requirement: List books in a library
The system SHALL return all books of a library on `GET /api/libraries/{libraryId}/books`, with an empty array when the library has no books, and HTTP 404 when the library does not exist. This endpoint SHALL be accessible without authentication.

#### Scenario: Library has books
- **WHEN** a client calls `GET /api/libraries/{libraryId}/books` for a library that has books
- **THEN** the system returns HTTP 200 with a JSON array containing those books

#### Scenario: Library has no books
- **WHEN** a client calls `GET /api/libraries/{libraryId}/books` for a library that exists but has no books
- **THEN** the system returns HTTP 200 with an empty JSON array

#### Scenario: Library does not exist
- **WHEN** a client calls `GET /api/libraries/{libraryId}/books` for a library that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Add a book to a library
The system SHALL create a book in a library on `POST /api/libraries/{libraryId}/books` and return the created book with HTTP 201, or HTTP 404 when the library does not exist.

#### Scenario: Library exists
- **WHEN** a client calls `POST /api/libraries/{libraryId}/books` with a valid book name and category
- **THEN** the system creates the book under that library, assigns it an id, and returns HTTP 201 with the created book

#### Scenario: Library does not exist
- **WHEN** a client calls `POST /api/libraries/{libraryId}/books` for a library that does not exist
- **THEN** the system does not create the book and returns HTTP 404

### Requirement: Update a book
The system SHALL update an existing book on `PUT /api/libraries/{libraryId}/books/{bookId}` and return HTTP 204, or HTTP 404 when the book or its library does not exist.

#### Scenario: Book exists
- **WHEN** a client calls `PUT /api/libraries/{libraryId}/books/{bookId}` for an existing book with updated name/category
- **THEN** the system persists the update and returns HTTP 204

#### Scenario: Book does not exist
- **WHEN** a client calls `PUT /api/libraries/{libraryId}/books/{bookId}` for a book that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Delete a book
The system SHALL delete a book on `DELETE /api/libraries/{libraryId}/books/{bookId}` and return HTTP 204, or HTTP 404 when the book does not exist. After deletion the book SHALL no longer be retrievable.

#### Scenario: Book exists
- **WHEN** a client calls `DELETE /api/libraries/{libraryId}/books/{bookId}` for an existing book
- **THEN** the system deletes the book and returns HTTP 204

#### Scenario: Book does not exist
- **WHEN** a client calls `DELETE /api/libraries/{libraryId}/books/{bookId}` for a book that does not exist
- **THEN** the system returns HTTP 404
