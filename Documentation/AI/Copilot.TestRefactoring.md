# Copilot – Unit Test Modernization

## Objective

Modernize all unit tests in the solution without changing their behavior.

This is a **pure refactoring**.

No production code must be modified.

No assertions must change.

No test logic must change.

The goal is to improve readability, consistency and modern C# style.

---

# Naming Convention

Rename every test using the following pattern:

```
<Method>_When<Condition>_<ExpectedResult>
```

Examples:

```
Kind_WhenParentIsNull_ReturnsGallery

Kind_WhenAlbumHasChildren_ReturnsCollection

GetPhoto_WhenPhotoDoesNotExist_ReturnsNull
```

Avoid names such as:

```
ShouldReturn...

Can...

Works...

Test1...
```

---

# AAA Pattern

Every test must follow the Arrange / Act / Assert pattern.

Use explicit section comments.

Example:

```csharp
// Arrange

...

// Act

...

// Assert

...
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

Avoid:

```csharp
var album = new Album();

album.ParentId = Guid.NewGuid();

album.Children.Add(new Album());
```

unless the object cannot reasonably be initialized directly.

---

# Collection Expressions

Use modern C# collection expressions.

Preferred:

```csharp
Children = []

Children = [ child ]

Children = [ first, second ]
```

When a collection contains a single element, keep it on a single line.

Preferred:

```csharp
Children = [ new Album() ]
```

Avoid:

```csharp
Children =
[
    new Album()
]
```

unless the line becomes excessively long.

---

# Modern C#

Use modern language features whenever they improve readability.

Examples:

- collection expressions
- target typed new
- nameof
- pattern matching
- expression-bodied members (only when clearly more readable)

Do not introduce language features simply because they are newer.

Readability always has priority.

---

# Assertions

Do not change assertion semantics.

Only improve formatting if necessary.

---

# Variables

Prefer `var` whenever the type is obvious.

Avoid unnecessary temporary variables.

---

# Formatting

Prefer compact, highly readable code.

Avoid unnecessary blank lines.

Keep Arrange sections visually compact.

Keep line lengths reasonable.

---

# Comments

Do not introduce explanatory comments.

The only comments that should normally appear are:

```csharp
// Arrange

// Act

// Assert
```

---

# Safety Rules

Do not modify production code.

Do not modify public APIs.

Do not change test behavior.

Do not change assertion logic.

If a refactoring could alter semantics, leave the original code unchanged.

---

# MPS Coding Philosophy

When multiple equivalent implementations are possible, always choose the one that maximizes readability for a human developer.

Do not optimize for the shortest code.

Do not optimize for clever code.

Optimize for maintainability.

Code should be understandable after several years by someone who has never seen it before.

---

# Expected Output

Generate a single commit containing only unit test refactoring.

No production code changes.