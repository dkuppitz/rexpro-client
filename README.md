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
var result = client.Query<long>("g.V.count()");
```

### Queries with complex return value

```C#
// not really different from scalar return values
var bindings = new Dictionary<string, object> {{ "name", "foo" }};
var result = client.Query<Vertex<Example>>("g.addVertex(['name':name])", bindings);
```

### Queries with sessions

```C#
using (var session = client.StartSession())
{
    client.Query("number = 1 + 2", session);
    var result = client.Query<int>("number", session);
}
```

### Dynamic queries

```C#
dynamic res1 = client.Query("1 + 2");
dynamic res2 = client.Query("g.addVertex(['foo':'bar'])")
dynamic res3 = client.Query("g.addVertex(['lorem':'ipsum']).map()")

Console.WriteLine("1 + 2 = {0}", res1);
Console.WriteLine("foo vertex id: {0}", res2._id);
Console.WriteLine("lorem: {0}", res3.lorem);
```
