DeepEqual
=
This is a Net Standard 2.0 compatible fork of James Foster's [DeepEqual][1].

DeepEqual is an extensible deep equality comparison library.

Installing
-

Install via NuGet

`Install-Package DeepEqual`

Usage
-

To test equality simply call the `IsDeepEqual` extension method.

```c#
bool result = object1.IsDeepEqual(object2);
```

When used inside a test you might want to call  `ShouldDeepEqual` instead. This method throws an exception with a detailed description of the differences between the 2 objects.

```c#
object1.ShouldDeepEqual(object2);
```

You can pass a custom comparison as the second argument to the `ShouldDeepEqual` method to override the default behaviour. You can also customize the behaviour inline using the `WithDeepEqual` extension method.

```c#
object1.WithDeepEqual(object2)
       .SkipDefault<MyEntity>()
       .IgnoreSourceProperty(x => x.Id)
       .Assert()
```

[1]: https://github.com/jamesfoster/DeepEqual