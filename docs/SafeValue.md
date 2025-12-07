# SafeValue API Documentation

`SafeValue<T>` is the core building block of the SafeMap pattern.  
It allows you to navigate object graphs WITHOUT null checks, if/else, or deep nesting.

---

## ✔ Purpose
- Avoid `if(obj != null)` everywhere  
- Avoid `?.?` chains that break when shape changes  
- Enforce readable, consistent mapping logic  
- Replace fragile ternaries with clean flow

---

## 🧱 Core Methods

### `SafeValue(T? input)`
Wraps any reference type safely.

### `Map<TResult>(Func<T, TResult?> projector)`
Transforms current value into another, safely.

### `Default(TResult fallback)`
If all previous mapping resulted in `null`, return a fallback.

### `Value()`
Extracts the final value.

---

## 🧪 Example (Before → After)

### ❌ Before (bad)
```csharp
var status = dealer?.Trial?.StatusId != null
    ? dealer.Trial.StatusId.Value.ToString()
    : "";
```
### ✔ After (good)
```csharp
var status = Safe.Guard(dealer)
    .Map(d => d.Trial)
    .Map<int>(t => t?.StatusId)
    .Map(s => s?.ToString())
    .Default("")
    .Value();

```
---
👍 Best Use Cases

Null-safe property navigation

Mapping domain → DTO

Optional relationships

Pre-validating inputs

Avoiding fragile chains
