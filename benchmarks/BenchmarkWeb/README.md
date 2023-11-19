# BenchmarkWeb

This has multiple compilation targets to simulate different frameworks.

Specifically:

- Carter
- ASP.NET Core MVC
- FastEndpoints
- ASP.NET Core Minimal API
- Reaper (non-AOT)
- Reaper (AOT)

The endpoint for high load is currently simplistic (in that it just returns "Hello, World!") and is located at `/ep`. In
the future, this may change a JSON endpoint for example (or MessagePack), or even better, be switchable for further tests.

**This needs work**: Ideally we want to simulate a "real world" app that has a reasonable amount of endpoints (~50) to
be wired at startup. This is not currently the case.