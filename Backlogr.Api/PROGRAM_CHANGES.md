Add these lines to `Program.cs`.

1. Add the options registration near the other options:

```csharp
builder.Services.Configure<DemoSeedOptions>(
    builder.Configuration.GetSection(DemoSeedOptions.SectionName));
```

2. Register the new service near the other scoped services:

```csharp
builder.Services.AddScoped<IDemoSeedService, DemoSeedService>();
```

3. Ensure these namespaces are included at the top if they are not already available:

```csharp
using Backlogr.Api.Options;
using Backlogr.Api.Services.Implementations;
using Backlogr.Api.Services.Interfaces;
```
