# .NET 7 Features
![Build](https://github.com/falberthen/NET7.Features/actions/workflows/net7-build.yml/badge.svg)

This project is a demo of some .NET 7 and ASP.NET Core 7 features.

- API RateLimiting middleware:
   - FixedWindowRateLimiter
   - ConcurrencyLimiter
   - SlidingWindowRateLimiter
   - TokenBucketRateLimiter
   - IRateLimiterPolicy
	
- Filters with minimal APIs.
- Route groups with minimal APIs.
- Typed results with minimal APIs.
- Output caching middleware.

---

You'll need to set up your machine to run .NET 7 or later, including the C# 10 or later compiler. 
The C# 10 compiler is available starting with <a href="https://visualstudio.microsoft.com/vs/">Visual Studio 2022</a> or the <a href="https://dotnet.microsoft.com/download">.NET 6 SDK</a>.