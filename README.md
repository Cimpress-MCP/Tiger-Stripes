# Tiger.Stripes

## What It Is

Tiger.Stripes is a .NET library for simplifying the configuration and development of AWS Lambda Functions written in C# and compiled to NativeAOT. It provides a common host allowing for configuration and dependency injection nearly identical to that of ASP.NET Core.

## Why You Want It

Even a non-complicated AWS Lambda Function can quickly gain a tedious amount of setup.
An `HttpClient` requires a set of `DelegatingHandler`s,
each of which requires its own set of dependencies,
some of which are `IOptions<TOptions>`,
and didn't Microsoft just release a library to _simplify_ HttpClient?

Tiger.Lambda provides a host very similar to the `WebApplicationHost` of ASP.NET Core,
allowing the application to be configured in all the ways familiar to an ASP.NET Core developer.
The most common actions are exposed as overrideable methods on the Function handler.
Even appsettings files are supported.

## How To Use It

This library _only_ targets .NET 8 and _only_ targets AOT compilation.
It simply will not work under the managed `dotnet8` runtime.
It must be deployed to a `provided.*` runtime.

The concept of a function handler is exposed by mapping a handler function to a name in the application builder.
The following _extremely simplified_ example illustrates a basic setup.

```csharp
var builder = DownloaderApplication.CreateBuilder();

_ = builder.Services.AddSingleton(TimeProvider.System);
_ = builder.Services
    .AddSingleton<IValidateOptions<VendingMachineOptions>, VendingMachineOptions.Validator>()
    .AddOptions<VendingMachineOptions>()
    .BindConfiguration(VendingMachineOptions.VendingMachine);

await using var app = builder.Build();
_ = app.MapInvoke(
    "Vendor",
    static (object _, IAmazonS3 s3, TimeProvider time, IOptions<VendingMachineOptions> o) =>
    {
        var now = time.GetUtcNow();
        var req = new GetPreSignedUrlRequest
        {
            BucketName = o.Value.BucketName,
            Expires = now.Add(o.Value.CredentialsDuration).UtcDateTime,
            Key = Ulid.NewUlid(now).ToString("G", CI.InvariantCulture),
            Verb = HttpVerb.PUT,
        };
        return Results.Created(s3.GetPreSignedURL(req));
    },
    AwsContext.Default);

await app.RunAsync();
```

Invocations map a function handler to a name, and also provide the means to serialize input and deserialize outputs.
This can be provided by a single instance of a type inheriting from `JsonSerializerContext` or by two instances of `JsonTypeInfo<T>`.

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
