using System.Collections.Generic;
using System.Linq;
using BenScr.Security.Cryptography;

namespace RandomCS.Tests
{
    /// <summary>
    /// Tests for the cryptographically-secure <see cref="RandomSecure"/> generator.
    /// This generator is not seedable, so tests verify ranges, distribution, validation and
    /// generic dispatch rather than reproducibility.
    /// </summary>
    [TestFixture]
    public class RandomSecureTests
    {
        // The secure generator is backed by the OS CSPRNG (slower than the xorshift generator),
        // so a smaller but still statistically comfortable sample is used.
        private const int Samples = 50_000;

        private RandomSecure _rng = null!;

        [SetUp]
        public void SetUp() => _rng = new RandomSecure();

        // ---------------------------------------------------------------------
        // NextInt
        // ---------------------------------------------------------------------

        [Test]
        public void NextInt_NoArgs_IsNonNegative()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextInt(), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void NextInt_WithMax_IsWithinRange()
        {
            const int max = 100;
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextInt(max), Is.GreaterThanOrEqualTo(0).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithMinMax_IsWithinRange()
        {
            const int min = 10, max = 20;
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextInt(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithNegativeRange_IsWithinRange()
        {
            const int min = -50, max = -10;
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextInt(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithRange_HitsEveryValue()
        {
            var seen = new HashSet<int>();
            for (int i = 0; i < Samples; i++)
                seen.Add(_rng.NextInt(0, 10));

            Assert.That(seen, Is.EquivalentTo(Enumerable.Range(0, 10)));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void NextInt_MaxNotPositive_Throws(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextInt(max));
        }

        [TestCase(5, 5)]
        [TestCase(10, 3)]
        public void NextInt_MinNotLessThanMax_Throws(int min, int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextInt(min, max));
        }

        // ---------------------------------------------------------------------
        // NextByte
        // ---------------------------------------------------------------------

        [Test]
        public void NextByte_WithMax_IsBelowMax()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextByte(100), Is.LessThan((byte)100));
        }

        [Test]
        public void NextByte_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextByte(20, 80), Is.GreaterThanOrEqualTo((byte)20).And.LessThan((byte)80));
        }

        [Test]
        public void NextByte_MaxZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextByte(0));
        }

        [TestCase((byte)10, (byte)10)]
        [TestCase((byte)200, (byte)50)]
        public void NextByte_MinNotLessThanMax_Throws(byte min, byte max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextByte(min, max));
        }

        // ---------------------------------------------------------------------
        // NextDouble / NextFloat
        // ---------------------------------------------------------------------

        [Test]
        public void NextDouble_NoArgs_IsInUnitInterval()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextDouble(), Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
        }

        [Test]
        public void NextDouble_WithMax_CoversFullRange()
        {
            const double max = 50.0;
            double observedMax = double.MinValue;
            double sum = 0;

            for (int i = 0; i < Samples; i++)
            {
                double v = _rng.NextDouble(max);
                Assert.That(v, Is.GreaterThanOrEqualTo(0.0).And.LessThan(max));
                if (v > observedMax) observedMax = v;
                sum += v;
            }

            Assert.That(observedMax, Is.GreaterThan(1.0), "Scaled values should extend beyond 1.0 toward max.");
            Assert.That(sum / Samples, Is.EqualTo(max / 2.0).Within(max * 0.03));
        }

        [Test]
        public void NextDouble_WithMinMax_IsWithinRange()
        {
            const double min = -5.0, max = 15.0;
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextDouble(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void NextDouble_MaxNotPositive_Throws(double max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextDouble(max));
        }

        [Test]
        public void NextFloat_NoArgs_IsInUnitInterval()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextFloat(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void NextFloat_WithMax_CoversFullRange()
        {
            const float max = 50f;
            float observedMax = float.MinValue;

            for (int i = 0; i < Samples; i++)
            {
                float v = _rng.NextFloat(max);
                Assert.That(v, Is.GreaterThanOrEqualTo(0f).And.LessThan(max));
                if (v > observedMax) observedMax = v;
            }

            Assert.That(observedMax, Is.GreaterThan(1f), "Scaled values should extend beyond 1.0 toward max.");
        }

        [Test]
        public void NextFloat_WithMinMax_IsWithinRange()
        {
            const float min = 1f, max = 4f;
            for (int i = 0; i < Samples; i++)
                Assert.That(_rng.NextFloat(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        public void NextFloat_MaxNotPositive_Throws(float max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rng.NextFloat(max));
        }

        // ---------------------------------------------------------------------
        // NextBool
        // ---------------------------------------------------------------------

        [Test]
        public void NextBool_IsApproximatelyFair()
        {
            int trues = 0;
            for (int i = 0; i < Samples; i++)
                if (_rng.NextBool()) trues++;

            Assert.That((double)trues / Samples, Is.EqualTo(0.5).Within(0.02));
        }

        // ---------------------------------------------------------------------
        // NextString
        // ---------------------------------------------------------------------

        [Test]
        public void NextString_DefaultLength_IsTen()
        {
            Assert.That(_rng.NextString().Length, Is.EqualTo(10));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(64)]
        public void NextString_RespectsRequestedLength(int length)
        {
            Assert.That(_rng.NextString(length).Length, Is.EqualTo(length));
        }

        [Test]
        public void NextString_DefaultCharset_UsesEveryAlphanumeric()
        {
            var seen = new HashSet<char>();
            for (int i = 0; i < 50; i++)
                foreach (char c in _rng.NextString(1000))
                    seen.Add(c);

            var expected = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            Assert.That(seen, Is.EquivalentTo(expected));
        }

        [Test]
        public void NextString_CustomCharset_UsesOnlyProvidedCharacters()
        {
            const string charset = "01";
            string s = _rng.NextString(500, charset);

            Assert.That(s, Has.Length.EqualTo(500));
            Assert.That(s.All(charset.Contains), Is.True);
        }

        // ---------------------------------------------------------------------
        // GenerateBytes
        // ---------------------------------------------------------------------

        [Test]
        public void GenerateBytes_FillsBuffer()
        {
            var buffer = new byte[1024];
            _rng.GenerateBytes(buffer);

            // An all-zero result from a 1 KB CSPRNG draw is astronomically unlikely (~2^-8192).
            Assert.That(buffer.Any(b => b != 0), Is.True);
            Assert.That(buffer.Length, Is.EqualTo(1024));
        }

        [Test]
        public void GenerateBytes_EmptyBuffer_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _rng.GenerateBytes(System.Array.Empty<byte>()));
        }

        // ---------------------------------------------------------------------
        // Generic Next<T> dispatch
        // ---------------------------------------------------------------------

        [Test]
        public void Next_Generic_ReturnsValueConsistentWithType()
        {
            Assert.That(_rng.Next<int>(), Is.GreaterThanOrEqualTo(0));
            Assert.That(_rng.Next<float>(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            Assert.That(_rng.Next<double>(), Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
            Assert.That(_rng.Next<string>(), Has.Length.EqualTo(10));
            Assert.DoesNotThrow(() => _rng.Next<byte>());
            Assert.DoesNotThrow(() => _rng.Next<bool>());
        }

        [Test]
        public void NextRange_Generic_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
            {
                Assert.That(_rng.Next<int>(3, 9), Is.GreaterThanOrEqualTo(3).And.LessThan(9));
                Assert.That(_rng.Next<double>(1.0, 4.0), Is.GreaterThanOrEqualTo(1.0).And.LessThan(4.0));
            }
        }

        [Test]
        public void Next_UnsupportedType_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => _rng.Next<long>());
        }

        [Test]
        public void NextRange_UnsupportedType_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => _rng.Next<long>(0L, 10L));
        }
    }
}
