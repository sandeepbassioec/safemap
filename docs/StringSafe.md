
---

# 📄 **5. `StringSafe.md`**
```md
# StringSafe API Documentation

StringSafe solves messy string checks:

### ❌ Classic
```csharp
if (string.IsNullOrWhiteSpace(name))
    return "Unknown";
return name.Trim();
```
### ✔ StringSafe
```csharp
var clean = StringSafe.Value(name)
    .Trim()
    .Default("Unknown")
    .Value();

```
---
Features
IsEmpty()

Checks null/empty/whitespace.

Trim()

Safe trimming.

Map(Func<string, string>)

Transforms the string.

Default(string fallback)

Fallback if empty.

Value()

Returns final output.
---

Complex Example
Before

```csharp
if (!string.IsNullOrWhiteSpace(code))
    code = code.Trim().ToUpper();
else
    code = "N/A";

```

After
```csharp
code = StringSafe.Value(code)
    .Trim()
    .Map(c => c.ToUpper())
    .Default("N/A")
    .Value();

```
