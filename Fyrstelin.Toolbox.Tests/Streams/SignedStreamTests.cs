using System.Security.Cryptography;
using System.Text;
using Fyrstelin.Toolbox.Streams;
using Shouldly;

namespace Fyrstelin.Toolbox.Tests.Streams;

public class SignedStreamTests
{
    private static readonly HashAlgorithm Hash = new HMACSHA1("my-secret"u8.ToArray());

    [Fact]
    public void ShouldSign()
    {
        var source = "Hello world"u8;

        using var memoryStream = new MemoryStream();
        using (var stream = SignedStream.ForWriting(memoryStream, Hash))
        {
            stream.Write(source);
        }

        var bytes = memoryStream.ToArray();

        Encoding.UTF8.GetString(bytes[..source.Length]).ShouldBe("Hello world");
        bytes[source.Length..].Length.ShouldBe(Hash.HashSize / 8);
    }

    [Fact]
    public void ShouldVerify()
    {
        var source = "Hello world"u8;
        using var memoryStream = Stream(source);

        using var stream = SignedStream.ForReading(memoryStream, Hash);
        var buffer = new byte[2];

        var i = 0;

        while (i < source.Length) 
        {
            var read = stream.Read(buffer, 0, buffer.Length);

            for (var j = 0; j < read; j++)
            {
                source[i + j].ShouldBe(buffer[j]);
            }

            i += read;
        }
    }

    [Fact]
    public void ShouldFailWhenDataHaveBeenTampered()
    {
        var data = "Some data"u8;
        var memoryStream = Stream(data);
        memoryStream.GetBuffer()[0] = 42;

        using var stream = SignedStream.ForReading(memoryStream, Hash);

        var buffer = new byte[data.Length-1];

        var read = stream.Read(buffer, 0, buffer.Length);
        read.ShouldBe(buffer.Length);

        Should.Throw<CryptographicException>(() => stream.Read(buffer, 0, 1));
    }

    private static MemoryStream Stream(ReadOnlySpan<byte> content)
    {
        var memoryStream = new MemoryStream();
        using (var stream = SignedStream.ForWriting(memoryStream, Hash))
        {
            stream.Write(content);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}