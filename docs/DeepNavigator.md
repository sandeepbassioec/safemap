
---

# 📄 **3. `DeepNavigator.md`**
```md
# DeepNavigator API Documentation

`DeepNavigator<T>` is used for readable, sequential navigation through deep object graphs.

It prevents nested maps like:
```csharp
Map().Map().Map().Map()

Instead you express the flow.

---
Constructor
DeepNavigator.Start(T input)

Starts a navigation chain.
---
Methods
Step<TNext>(Func<T, TNext?> projector)

Go to the next step in the deep graph.

Default(T value)

Return fallback at end.

Value()

Return final value.
---

Example
❌ Before

```csharp
var city = person?.Profile?.Address?.City?.Name ?? "Unknown";

```
✔ After
```csharp
var city = DeepNavigator.Start(person)
    .Step(p => p.Profile)
    .Step(pr => pr.Address)
    .Step(a => a.City)
    .Step(c => c.Name)
    .Default("Unknown")
    .Value();

```

