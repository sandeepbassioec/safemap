
---

# 📄 **2. `SafeValueStruct.md`**
```md
# SafeValueStruct API Documentation

`SafeValueStruct<T>` is the struct-based variant of SafeValue, designed for handling `Nullable<T>` and value-based workflows.

---

## Purpose
- Handle `int?`, `DateTime?`, enums
- Null-safe conversion
- Deep struct transformation

---

## Core Methods

### `MapNullable<TResult>(Func<T, TResult?> projector)`
Transforms the underlying struct into a new nullable struct.

### `MapToRef<TResult>(Func<T, TResult?> projector)`
Projects struct → reference type.

### `Default(TResult value)`
Fallback when null.

### `Value()`
Returns the final struct value.

---

## Example

### ❌ Before
```csharp
bool expired = trial?.TrialEndAt != null
    ? trial.TrialEndAt < DateTime.UtcNow
    : false;
```
### ✔ After
```csharp
var expired = Safe.Guard(trial)
    .Map(t => t.TrialEndAt)
    .MapNullable(dt => dt < DateTime.UtcNow)
    .Default(false)
    .Value();

```
