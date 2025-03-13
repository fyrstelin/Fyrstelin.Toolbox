using System.Security.Cryptography;

namespace Fyrstelin.Toolbox.Streams
{
    public sealed class SignedStream : Stream
    {
        private readonly Stream _decoratee;
        private readonly HashAlgorithm _hash;
        private readonly bool _writer;

        private SignedStream(Stream decoratee, HashAlgorithm hash, bool writer)
        {
            _decoratee = decoratee;
            _hash = hash;
            _writer = writer;
        }

        public static SignedStream ForWriting(Stream decoratee, HashAlgorithm hash) => new(decoratee, hash, true);
        public static SignedStream ForReading(Stream decoratee, HashAlgorithm hash) => new(decoratee, hash, false);

        public override void Flush()
        {
            _decoratee.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _writer)
            {
                _hash.TransformFinalBlock([], 0, 0);
                _decoratee.Write(_hash.Hash);
            }
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_writer)
                throw new InvalidOperationException($"Cannot read from a writable {nameof(SignedStream)}");

            var actualCount = Math.Min(count, (int)(_decoratee.Length - _decoratee.Position - _hash.HashSize / 8));

            var bytesRead = _decoratee.Read(buffer, offset, actualCount);
            _hash.TransformBlock(buffer, offset, actualCount, null, 0);

            if (_decoratee.Length - _decoratee.Position == _hash.HashSize / 8)
            {
                _hash.TransformFinalBlock([], 0, 0);
                var expected = _hash.Hash;
                var actual = new byte[_hash.HashSize / 8];
                var read = _decoratee.Read(actual, 0, actual.Length);
                
                if (read != actual.Length) throw new InvalidOperationException();

                if (!CryptographicOperations.FixedTimeEquals(expected, actual))
                {
                    throw new CryptographicException("Signature verification failed!");
                }
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException($"Cannot seak in a {nameof(SignedStream)}");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException($"Cannot set length on a {nameof(SignedStream)}");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_writer) throw new NotSupportedException($"Cannot write to a readable {nameof(SignedStream)}");
            _decoratee.Write(buffer, offset, count);
            _hash.TransformBlock(buffer, offset, count, null, 0);
        }

        public override bool CanRead => !_writer && _decoratee.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _writer && _decoratee.CanWrite;
        public override long Length => _decoratee.Length;
        public override long Position
        {
            get => throw new InvalidOperationException($"Cannot get the position of a {nameof(SignedStream)}");
            set => throw new InvalidOperationException($"Cannot set the position of a {nameof(SignedStream)}");
        }
    }
}
