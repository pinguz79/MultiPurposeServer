# Copilot – MPS Development Playbook

## Objective

Generate production-quality code consistent with the MPS architecture.

When multiple implementations are possible, always choose the one that:

- maximizes readability;
- minimizes long-term maintenance cost;
- follows existing project patterns;
- preserves architectural consistency.

When in doubt, prefer consistency with the existing codebase over introducing a new pattern.

---

# MPS Coding Philosophy

Readability always comes before brevity.

Do not optimize for the shortest code.

Do not optimize for clever code.

Optimize for maintainability.

Code should be understandable after several years by someone who has never seen it before.

Prefer explicit code over implicit code.

Prefer simple algorithms over generic abstractions.

Introduce abstractions only after observing real duplication.

Avoid speculative abstractions.

---

# Architecture

Respect the project layering.

Controllers

↓

Application

↓

Infrastructure

↓

Persistence

Responsibilities:

- Controllers orchestrate use cases.
- Application contains business logic.
- Infrastructure contains technical implementations.
- Persistence contains data access.

Controllers must never contain persistence logic.

Repositories must never contain business logic.

---

# Repository Philosophy

Extract common algorithms.

Do not extract domain behavior.

Good candidates for BaseRepository:

- transaction management
- SaveIfRequired
- validation helpers
- FindById
- Update
- GetAll
- GetByIds

Keep domain-specific queries inside concrete repositories.

Avoid implementing a generic CRUD repository.

---

# Refactoring Strategy

Perform refactorings incrementally.

Preferred workflow:

1. One small refactoring.
2. Build.
3. Run all unit tests.
4. Commit.
5. Proceed to the next refactoring.

Avoid combining unrelated architectural changes into a single commit.

---

# Refactoring Completion Checklist

Before considering a refactoring complete:

☐ Solution builds successfully.

☐ All unit tests pass.

☐ Folder structure matches namespaces.

☐ All namespaces match the folder hierarchy.

☐ All affected using directives have been updated.

☐ No obsolete files remain.

☐ No obsolete interfaces remain.

☐ No obsolete methods remain.

☐ No obsolete using directives remain.

☐ Every TODO introduced during the refactoring has been reviewed.

☐ Public methods introduced or modified are covered by unit tests.

☐ Validation paths are covered by tests.

☐ Exception paths are covered by tests.

☐ Edge cases are covered by tests.

☐ Explicitly report any uncovered methods or scenarios.

☐ Tests do not verify responsibilities belonging to another architectural layer.

☐ Pass-through services are tested only for correct delegation.

☐ Persistence behavior is verified only in repository or integration tests.

☐ HTTP behavior is verified only in controller tests.

☐ Duplicate assertions across layers have been reviewed and removed where unnecessary.

---

# Folder / Namespace Synchronization

Whenever folders or files are moved:

- Verify that every namespace matches the physical folder structure.
- Verify that all affected using directives have been updated.
- Never leave the project in a partially synchronized state.
- Report any inconsistency before completing the refactoring.

---

# Test Layer Responsibility

Each test must verify only the responsibilities of the layer under test.

## Entity Tests

Verify:

- computed properties;
- entity state transitions;
- entity-specific validation;
- entity behavior.

Do not verify repository, service or controller behavior.

## Repository Tests

Verify:

- database queries;
- persistence;
- updates;
- transactions;
- commit and rollback;
- missing entities;
- repository-level validation.

Repository tests may use a real test database provider when persistence behavior must be verified.

## Service Tests

Verify:

- correct delegation to repositories or other dependencies;
- application logic owned by the service;
- correct parameters passed to dependencies;
- correct propagation or transformation of results and exceptions.

Do not verify whether the repository actually persists or updates data.

If a service is a pure pass-through, its test should remain a delegation test.

## Controller Tests

Verify:

- request validation;
- normalization;
- orchestration of service calls;
- operation lifecycle;
- HTTP status codes;
- response mapping.

Do not verify repository persistence or internal service implementation.

## No Cross-Layer Duplication

Avoid duplicating the same behavioral assertion across multiple layers.

A defect in one layer should fail primarily the tests responsible for that layer.

Example:

- If a repository update does not persist the new value, repository tests must fail.
- If the service still calls the repository correctly, service tests should continue to pass.
- If the controller calls the service correctly, controller tests should continue to pass.

---

# Test Coverage Review

After every completed feature or refactoring:

- Analyze the entire unit test suite.
- Identify public methods without unit tests.
- Identify uncovered execution paths.
- Identify missing validation scenarios.
- Identify missing exception scenarios.
- Identify missing edge cases.
- Suggest additional tests before considering the task complete.

Do not assume similar methods imply sufficient coverage.

---

# Unit Test Naming Convention

Rename every test using the following pattern:

<Method>_When<Condition>_<ExpectedBehavior>

Examples:

Kind_WhenParentIsNull_ReturnsGallery

Kind_WhenAlbumHasChildren_ReturnsCollection

Commit_WhenTransactionIsDisposed_ThrowsObjectDisposedException

Update_WhenDescriptionIsMissing_ThrowsArgumentException

Avoid names such as:

Should...

Can...

Works...

Test1...

---

# AAA Pattern

Every test must follow the Arrange / Act / Assert pattern.

Use explicit section comments.

```csharp
// Arrange

// Act

// Assert
```

---

# Arrange

Prefer object initializers whenever possible.

Good:

```csharp
var album = new Album
{
    ParentId = Guid.NewGuid(),
    Children = [ new Album() ]
};
```

Avoid property-by-property initialization unless necessary.

---

# Collection Expressions

Prefer modern C# collection expressions.

Good:

```csharp
Children = []

Children = [ child ]

Children = [ first, second ]
```

When a collection contains a single element keep it on a single line whenever readable.

---

# Modern C#

Use modern language features only when they improve readability.

Good candidates:

- collection expressions
- nameof
- pattern matching
- target typed new
- expression-bodied members (when clearly more readable)

Do not use newer syntax simply because it exists.

---

# Variables

Prefer explicit types whenever they improve readability.

Use `var` only when the type is immediately obvious from the right-hand side.

Avoid unnecessary temporary variables.

---

# Formatting

Prefer compact, highly readable code.

Avoid unnecessary blank lines.

Avoid unnecessary line breaks.

Keep related statements visually grouped.

Arrange sections should remain compact.

---

# Comments

Avoid explanatory comments.

Normally the only comments should be:

```csharp
// Arrange

// Act

// Assert
```

---

# Safety Rules

Never modify production behavior during a refactoring.

Never modify public APIs unless explicitly requested.

Never change assertion semantics.

Never change test behavior.

If a refactoring could change semantics, stop and report the issue.

---

# Expected Output

Unless explicitly requested otherwise:

- perform one logical refactoring at a time;
- keep commits small and cohesive;
- preserve project consistency;
- report any architectural inconsistency found during the work.