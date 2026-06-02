using System.Security.Cryptography;
using System.Text;
using static BenScr.Text.Characters;

namespace BenScr.Security.Cryptography
{
    public sealed class RandomSecure
    {
        private static readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

        public bool NextBool() => NextInt(0, 2) == 0;
        public byte NextByte()
        {
            return (byte)(NextInt() & 0xFF);
        }
        public byte NextByte(byte max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return (byte)(NextByte() % max);
        }
        public byte NextByte(byte min, byte max)
        {
            if (min >= max) throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}).");
            return (byte)(min + (NextByte() % (max - min)));
        }

        public int NextInt()
        {
            byte[] fourBytes = new byte[4];
            randomNumberGenerator.GetBytes(fourBytes);
            int value = BitConverter.ToInt32(fourBytes, 0) & int.MaxValue;
            return value;
        }
        public int NextInt(int min, int max)
        {
            if (min >= max) throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}).");
            // Use long arithmetic for the span so the full int range is supported.
            return (int)(min + (NextInt() % ((long)max - min)));
        }
        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return NextInt() % max;
        }

        public double NextDouble()
        {
            byte[] eightBytes = new byte[8];
            randomNumberGenerator.GetBytes(eightBytes);
            ulong value = BitConverter.ToUInt64(eightBytes, 0);
            // Use the top 53 bits (the mantissa width of a double) for a uniform [0, 1).
            return (value >> 11) * (1.0 / (1UL << 53));
        }
        public double NextDouble(double max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return NextDouble() * max;
        }
        public double NextDouble(double min, double max)
        {
            return min + (NextDouble() * (max - min));
        }

        public float NextFloat()
        {
            byte[] fourBytes = new byte[4];
            randomNumberGenerator.GetBytes(fourBytes);
            uint value = BitConverter.ToUInt32(fourBytes, 0);
            // Use the top 24 bits (the mantissa width of a float) for a uniform [0, 1).
            return (value >> 8) * (1.0f / (1 << 24));
        }
        public float NextFloat(float max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return NextFloat() * max;
        }
        public float NextFloat(float min, float max)
        {
            return min + (NextFloat() * (max - min));
        }

        public string NextString(int length = 10, string? charset = null)
        {
            charset ??= CHARS;
            int charsetLength = charset.Length;

            StringBuilder code = new StringBuilder(length < 0 ? 0 : length);

            for (int i = 0; i < length; i++)
                code.Append(charset[NextInt(charsetLength)]);

            return code.ToString();
        }

        public T Next<T>() where T : IComparable<T>
        {
            if (typeof(T) == typeof(int))
            {
                int result = NextInt();
                return (T)(object)result;
            }

            if (typeof(T) == typeof(float))
            {
                float result = NextFloat();
                return (T)(object)result;
            }

            if (typeof(T) == typeof(double))
            {
                double result = NextDouble();
                return (T)(object)result;
            }

            if (typeof(T) == typeof(byte))
            {
                byte result = NextByte();
                return (T)(object)result;
            }

            if (typeof(T) == typeof(bool))
            {
                bool result = NextBool();
                return (T)(object)result;
            }

            if (typeof(T) == typeof(string))
            {
                string result = NextString();
                return (T)(object)result;
            }

            throw new NotSupportedException($"Type '{typeof(T)}' is not supported.");
        }

        public T Next<T>(T min, T max) where T : IComparable<T>
        {
            if (typeof(T) == typeof(int))
            {
                int result = NextInt(Convert.ToInt32(min), Convert.ToInt32(max));
                return (T)(object)result;
            }

            if (typeof(T) == typeof(float))
            {
                float result = NextFloat(Convert.ToSingle(min), Convert.ToSingle(max));
                return (T)(object)result;
            }

            if (typeof(T) == typeof(double))
            {
                double result = NextDouble(Convert.ToDouble(min), Convert.ToDouble(max));
                return (T)(object)result;
            }

            if (typeof(T) == typeof(byte))
            {
                byte result = NextByte(Convert.ToByte(min), Convert.ToByte(max));
                return (T)(object)result;
            }

            throw new NotSupportedException($"Type '{typeof(T)}' is not supported.");
        }

        public void GenerateBytes(byte[] bytes) => randomNumberGenerator.GetBytes(bytes);
    }
}
