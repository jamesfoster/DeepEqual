DeepEqual
=

DeepEqual is an extensible deep equality comparison library.

Installing
-

Install via NuGet (https://www.nuget.org/packages/DeepEqual/)

`Install-Package DeepEqual`

Usage
-

To test equality simply call the `IsDeepEqual` extension method.

```c#
bool result = left.IsDeepEqual(right);
```

When used inside a test you might want to call  `ShouldDeepEqual` instead. This method throws an exception with a detailed description of the differences between the 2 objects.

```c#
left.ShouldDeepEqual(right);

// or

left.ShouldDeepEqual(right, comparrison);
```

You can pass a custom comparison as the second argument to the `ShouldDeepEqual` method to override the default behaviour. You can also customize the behaviour inline using the `WithDeepEqual` extension method.

```c#
left.WithDeepEqual(right)
       .SkipDefault<MyEntity>()
       .IgnoreLeftProperty(x => x.Id)
       .Assert()
```


