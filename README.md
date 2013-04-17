rexpro-client
==============

RexPro Client Library for .NET

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

```C#
var client = new RexProClient();

var script1 = "g.addVertex(['name':name]); null";
var binding1 = new Tuple<string, object>("name", "v1");

client.Query(script1, binding1);

var script2 = "g.addVertex(['name':name]); null";
var binding2 = new KeyValuePair<string, object>("name", "v2");

client.Query(script2, binding2);

var script3 = "g.addVertex(['name':name1]); g.addVertex(['name':name2]); null";
var binding3 = new Dictionary<string, object>
{
    { "name1", "v3" },
    { "name2", "v4" }
};

client.Query(script3, binding3);

var script4 = new ScriptRequest("g.addVertex(['name':name1]); g.addVertex(['name':name2]); null")
  .AddBinding("name1", "foo")
  .AddBinding("name2", "bar");

client.ExecuteScript(script4);

var v1 = client.Query<Vertex<Example>>("g.V('name','v1').next()").Result;
var v2 = client.Query<Example>("g.V('name',name).next().map()", binding2).Result;
var count = client.Query<long>("g.V.count()").Result;
```
