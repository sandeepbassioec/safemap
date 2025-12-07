<img width="1536" height="1024" alt="SafeMapX" src="https://github.com/user-attachments/assets/fa7e75c3-bd03-4397-bf0d-74d99fac8a82" />

# SafeMapX  
**The Smartest Way to Map Values Safely.**

[![NuGet](https://img.shields.io/badge/NuGet-SafeMapX-blue?logo=nuget)]()
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen)]()
[![License](https://img.shields.io/badge/License-MIT-lightgrey.svg)]()

---
# SafeMapX

SafeMapX is a small utility library that provides a safer way to perform value mapping and conditional transformations in C#.
It is intended for situations where values may be null, optional, or conditionally valid, and where the typical approach tends to accumulate repetitive null checks, nested conditionals, or defensive code.

The goal is to make these mappings more readable and predictable without introducing new abstractions beyond simple fluent operations.

---

## Overview

Most C# applications contain code that looks roughly like:

```csharp
var value = source != null && source.Child != null
    ? source.Child.Name
    : "Unknown";
```

Or:

```csharp
var isExpired = record != null && record.EndDate.HasValue
    ? record.EndDate < DateTime.UtcNow
    : false;
```

SafeMapX provides a structured way to express the same intent without the branching clutter.
The API focuses on three ideas:

1. Start with a value that might not exist.
2. Apply transformations only when the previous step produced a usable value.
3. Provide an explicit fallback when the chain cannot produce one.

The pattern is simple and predictable, and it avoids exceptions that typically occur when dereferencing deeper properties.

---

## Basic Example

```csharp
var name = Safe
    .Map(() => user.Profile.FullName)
    .Or("Unknown");
```

This expresses the intent directly: “take this value if it exists; otherwise use a fallback.”

Another example with a conditional:

```csharp
var discount = Safe
    .Map(() => order.Total)
    .When(t => t > 500)
    .Map(t => t * 0.10)
    .Or(0);
```

Only when the value exists and satisfies the condition does the subsequent mapping run.

---

## Deep Property Access

SafeMapX is often useful for deeply nested properties:

```csharp
var city = Safe
    .Map(() => user.Address.Location.City)
    .Or("N/A");
```

If any segment of the chain is null, the fallback is used.

---

## Async Example

SafeMapX also supports async accessors:

```csharp
var record = await Safe
    .MapAsync(() => repository.GetAsync(id))
    .MapAsync(r => ProcessAsync(r))
    .OrAsync(defaultValue);
```

---

## Installation

SafeMapX is distributed as a NuGet package:

```
dotnet add package SafeMapX
```

The NuGet page will include version history and update notes after publishing.

---

## Test Coverage

The project includes unit tests covering:

* Null-path behavior
* Conditional (`When`) behavior
* Fallback execution
* Deep-member access
* Chaining
* Async mapping paths
* Exception handling within mapping delegates

The libraries and tests are structured to allow additional operator behavior to be added incrementally.

---

## Project Structure

```
SafeMapX/
    Safe/
        Safe.cs
        SafeMapper.cs
        Extensions/
    tests/
        SafeMapX.Tests/
    README.md
```

---

## Use Cases

SafeMapX is intended for:

* DTO to domain projections
* API response shaping
* Null-prone data access layers
* Optional configuration values
* Conditional transformation logic
* Eliminating nested ternary blocks
* Reducing repetition in validation-before-mapping steps

It is deliberately small, dependency-free, and suitable for use in existing projects without architectural changes.

[SafeValue API Documentation](docs/SafeValue.md)

[SafeValueStruct API Documentation](docs/SafeValueStruct.md)

[DeepNavigator API Documentation](docs/DeepNavigator.md)

[DeepPath API Documentation](docs/DeepPath.md)

[EnumSafe API Documentation](docs/EnumSafe.md)

[StringSafe API Documentation](docs/StringSafe.md)

---

## Contributing

Contributions are welcome.
Please open an issue if you encounter unexpected behavior or if you want to propose a new mapping operator.
The core idea should remain small and predictable, but improvements to clarity and ergonomics are always considered.

---

## License

SafeMapX is released under the MIT license.

