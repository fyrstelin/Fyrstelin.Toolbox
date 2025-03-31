# Fyrstelin.Toolbox.Streams

Utility (singular for now) for handling streams

## SignedStream

A simple stream that appends a signature to the end.

```c#
using var memoryStream = new MemoryStream();
using (var stream = SignedStream.ForWriting(memoryStream, new HMACSHA256("my-secret"u8.ToArray()))) {
  stream.Write([13, 37])
}

var data = memoryStream.ToArray();
//  ^? [13, 37 /* and 256 bits of signatyre*/]
```

Upon read, it verifies the signature when the last byte is read.

```c#
using var memoryStream = new MemoryStream(data);
using (var stream = SignedStream.ForReading(memoryStream, new HMACSHA256("my-secret"u8.ToArray()))) {
  var span = new byte[2].AsSpan();
  stream.Read(span);

  
}
```

