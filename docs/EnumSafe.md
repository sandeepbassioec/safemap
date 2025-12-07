
---

# 📄 **6. `EnumSafe.md`**
```md
# EnumSafe API Documentation

`EnumSafe<TEnum>` lets you safely map integers/strings to enums WITHOUT exceptions.

---

## Use Cases
- Safe enum casting
- Handling legacy db values
- Avoiding invalid enum crashes

---

## Methods

### `FromInt<TEnum>(int? value)`
Convert nullable int → enum?

### `FromString<TEnum>(string? value)`
Convert string → enum?

### `Default(TEnum fallback)`
Fallback for invalid input.

### `Value()`
Return final enum.

---

## Example

### ❌ Before
```csharp
DealerStatus status;
if (id == null || !Enum.IsDefined(typeof(DealerStatus), id))
    status = DealerStatus.Unknown;
else
    status = (DealerStatus)id.Value;
```
### ✔ After
```csharp
var status = EnumSafe.FromInt<DealerStatus>(id)
    .Default(DealerStatus.Unknown)
    .Value();

```
