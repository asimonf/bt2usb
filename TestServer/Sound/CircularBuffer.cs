using System;

namespace TestServer.Sound
{
    public class CircularBuffer<T> where T: unmanaged
    {
        private readonly T[] _backingBuffer;

        private int _start;
        private int _end;

        public int Capacity => _backingBuffer.Length;
        public int CurrentLength => _end >= _start ? _end - _start : _end + Capacity - _start;
        public int Glitches { get; set; }
        

        /// <summary>
        /// Creates a circular buffer
        /// </summary>
        /// <param name="size">Size of the buffer in samples</param>
        public CircularBuffer(int size)
        {
            _backingBuffer = new T[size];
            _start = 0;
            _end = 0;
            Glitches = 0;
        }

        public void AddSample(T sample)
        {
            if (_end == Capacity)
            {
                _backingBuffer[0] = sample;

                _end = 1;
            }
            else
            {
                
                _end += 1;
            }
        }

        public unsafe void CopyFrom(T* src, int length)
        {
            if (_end + length > Capacity)
            {
                var newLength = Capacity - _end;
                var remainder = length - newLength;

                // Buffer.BlockCopy(arr, 0, _backingBuffer, _end, newLength);
                fixed (T* dst = _backingBuffer)
                {
                    var newLengthBytes = sizeof(T) * newLength;
                    var remainderBytes = sizeof(T) * remainder;
                    Buffer.MemoryCopy(src, &dst[_end], newLengthBytes, newLengthBytes);
                    Buffer.MemoryCopy(&src[newLength], dst, remainderBytes, remainderBytes);
                }

                _end = remainder;
            }
            else
            {
                fixed (T* dst = _backingBuffer)
                {
                    var lengthBytes = sizeof(T) * length;
                    Buffer.MemoryCopy(src, &dst[_end], lengthBytes, lengthBytes);
                }
                _end = (_end + length) % Capacity;
            }
        }
        
        public void CopyFrom(T[] arr, int length)
        {
            if (_end + length > Capacity)
            {
                var newLength = Capacity - _end;
                var remainder = length - newLength;

                // Buffer.BlockCopy(arr, 0, _backingBuffer, _end, newLength);
                Buffer.BlockCopy(arr, 0, _backingBuffer, _end, newLength);
                Buffer.BlockCopy(arr, newLength, _backingBuffer, 0, remainder);

                _end = remainder;
            }
            else
            {
                Buffer.BlockCopy(arr, 0, _backingBuffer, _end, length);
                _end = (_end + length) % Capacity;
            }
        }

        public void CopyTo(T[] destination, int length)
        {
            // Zero-fill if the request can't be filled with the current buffer contents
            if (length > CurrentLength)
            {
                Glitches++;
                Console.Write(CurrentLength);
                Console.Write(',');
                Console.Write(length);
                Console.Write('.');

                return;
            }

            if (_start + length > Capacity)
            {
                var newLength = Capacity - _start;
                var remainder = length - newLength;

                Buffer.BlockCopy(_backingBuffer, _start, destination, 0, newLength);
                Buffer.BlockCopy(_backingBuffer, 0, destination, newLength, remainder);

                _start = remainder;
            }
            else if (length > 0)
            {
                Buffer.BlockCopy(_backingBuffer, _start, destination, 0, length);

                _start = (_start + length) % Capacity;
            }
        }

        public unsafe void CopyTo(T* destination, int length)
        {
            // Zero-fill if the request can't be filled with the current buffer contents
            if (length > CurrentLength)
            {
                Glitches++;
                Console.Write(CurrentLength);
                Console.Write(',');
                Console.Write(length);
                Console.Write('.');

                return;
            }

            if (_start + length > Capacity)
            {
                var newLength = Capacity - _start;
                var remainder = length - newLength;

                fixed (T* backPtr = _backingBuffer)
                {
                    var newLengthBytes = sizeof(T) * newLength;
                    var remainderBytes = sizeof(T) * remainder;
                    
                    Buffer.MemoryCopy(&backPtr[_start], destination, newLengthBytes, newLengthBytes);
                    Buffer.MemoryCopy(backPtr, &destination[newLength], remainderBytes, remainderBytes);
                    
                }

                _start = remainder;
            }
            else if (length > 0)
            {
                fixed (T* backPtr = _backingBuffer)
                {
                    var lengthBytes = length * sizeof(T);
                    Buffer.MemoryCopy(&backPtr[_start], destination, lengthBytes, lengthBytes);
                }

                _start = (_start + length) % Capacity;
            }
        }
    }
}