
---

# 📄 **4. `DeepPath.md`**
```md
# DeepPath API Documentation

`DeepPath<T>` allows you to define an entire access path in ONE place, avoiding repeated navigation logic.

---

## Purpose
- Entire safe navigation path in one expression
- Reusable across services
- Great for DTO extraction

---

## Constructor

### `DeepPath.Start(T input)`

---

## Methods

### `Go<TNext>(Func<T, TNext?> projector)`
Add a node to the path.

### `Default(TResult fallback)`
Set fallback if null at end.

### `Resolve()`
Executes the full path.

---

## Example

### ❌ Before
```csharp
string city = null;
if (person != null &&
    person.Profile != null &&
    person.Profile.Address != null &&
    person.Profile.Address.City != null)
{
    city = person.Profile.Address.City.Name;
}
else city = "Unknown";
```
### ✔ After
```csharp
var city = DeepPath.Start(person)
    .Go(p => p.Profile)
    .Go(pr => pr.Address)
    .Go(a => a.City)
    .Go(c => c.Name)
    .Default("Unknown")
    .Resolve();
```
