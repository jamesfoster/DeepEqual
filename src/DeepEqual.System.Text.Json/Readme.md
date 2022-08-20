DeepEqual.System.Text.Json
=

DeepEqual.System.Text.Json is extension to DeepEqual for comparing JSON documents.

Installing
-

Install via NuGet

`Install-Package DeepEqual.System.Text.Json`

Usage
-

1. Build a custom comparison and pass it to the deep equal methods

```c#
var comparison = new ComparisonBuilder()
    .UseSytemTextJson()
    .Create()

bool result = object1.IsDeepEqual(object2, comparison);

object1.ShouldDeepEqual(object2, comparison);
```

2. Customize the comparison builder inline

```c#
bool result = object1.WithDeepEqual(object2)
    .UseSystemTextJson()
    .Compare()

object1.WithDeepEqual(object2)
    .UseSystemTextJson()
    .Assert()
```

3. (NOT RECOMMENDED) Register the customization globally.

```c#
ComparisonBuilderExtensions.GloballyUseSystemTextJson();

bool result = object1.IsDeepEqual(object2);

object1.ShouldDeepEqual(object2);
```

The reason this is not recommended is it will affect all tests and could cause
internittent test failures when run in parallel.
N.B. To remove global customizations call ComparisonBuilder.Reset()

