rexster-client
==============

Rexster Client Library for .NET

## Usage

```C#
var client = new RexsterClient();
var count = client.Query<long>("g.V.count()").Result;
```
