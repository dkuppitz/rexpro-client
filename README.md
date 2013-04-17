rexpro-client
==============

RexPro Client for .NET

## getting started

First create a simple class to hold some vertex data.

```C#
[DataContract]
public class Example
{
    [DataMember(Name = "name")]
    public string Name { get; set; }
}
```

That's it. Now fire up some queries.

### Queries without return value (null)

```C#
var client = new RexProClient();

client.Query("g.addVertex(['name':'foo']); null");

// same query with parameter binding
var bindings = new Dictionary<string, object> {{ "name", "foo" }};
client.Query("g.addVertex(['name':name]); null", bindings);
```

### Queries with scalar return value

```C#
var result = client.Query<long>("g.V.count()").Result;

// or make use of automatic type casting
long count = client.Query<long>("g.V.count()");
```

### Queries with complex return value

```C#
// not really different from scalar return values
var bindings = new Dictionary<string, object> {{ "name", "foo" }};
var result = client.Query<Vertex<Example>>("g.addVertex(['name':name])", bindings).Result;

// again you can use automatic type casting (this time an explicit cast)
var example = (Example)client.Query<Vertex<Example>>("g.addVertex(['name':name]).map()", bindings);
```

### Queries with sessions

The following example should work in theory. In practice there's still a bug in session management, that will hopefully be fixed soon.

```C#
using (var session = client.OpenSession())
{
    client.Query("number = 1 + 2; null", session);
    var result = client.Query<int>("number", session);
}
```
