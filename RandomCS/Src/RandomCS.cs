using System.Diagnostics;
using System.Text;
using static BenScr.Text.Characters;

namespace BenScr.Random
{
    public sealed class RandomCS
    {
        // The xorshift* algorithm collapses to an all-zero sequence if the state ever
        // becomes zero, so a zero seed is remapped onto this fixed non-zero constant
        // (the 64-bit golden-ratio constant) to keep the generator well behaved.
        private const ulong DefaultSeed = 0x9E3779B97F4A7C15UL;

        private ulong state;

        public RandomCS(ulong seed)
        {
            SetSeed(seed);
        }

        public RandomCS()
        {
            RemoveSeed();
        }

        private ulong NextState()
        {
            state ^= state >> 12;
            state ^= state << 25;
            state ^= state >> 27;
            return state * 2685821657736338717UL;
        }

        public void SetSeed(ulong seed)
        {
            state = seed != 0 ? seed : DefaultSeed;
        }
        public void RemoveSeed()
        {
            ulong t = (ulong)DateTime.Now.Ticks;
            ulong g = (ulong)Guid.NewGuid().GetHashCode();
            ulong s = (ulong)Stopwatch.GetTimestamp();
            SetSeed(t ^ g ^ s);
        }

        public ulong GetSeed() => state;

     
        public bool NextBool() => NextInt(0, 2) == 0;

        public byte NextByte()
        {
            return (byte)(NextState() & 0xFF);
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
            return (int)(NextState() & 0x7FFFFFFF);
        }
        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return NextInt() % max;
        }
        public int NextInt(int min, int max)
        {
            if (min >= max) throw new ArgumentOutOfRangeException(nameof(min), $"min ({min}) must be less than max ({max}).");
            // Use long arithmetic for the span so the full int range is supported.
            return (int)(min + (NextInt() % ((long)max - min)));
        }

        public double NextDouble()
        {
            // Use the top 53 bits (the mantissa width of a double) to produce a uniform
            // value in the half-open interval [0, 1); this never returns exactly 1.0.
            return (NextState() >> 11) * (1.0 / (1UL << 53));
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
            // Use the top 24 bits (the mantissa width of a float) to produce a uniform
            // value in the half-open interval [0, 1); this never returns exactly 1.0f.
            return (NextState() >> 40) * (1.0f / (1 << 24));
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
    }
}
