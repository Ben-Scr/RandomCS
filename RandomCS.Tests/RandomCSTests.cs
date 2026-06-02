using System.Collections.Generic;
using System.Linq;
using BenScr.Random;

namespace RandomCS.Tests
{
    /// <summary>
    /// Tests for the fast, seedable (non-cryptographic) <see cref="BenScr.Random.RandomCS"/> generator.
    /// Because the generator is deterministic for a given seed, most behavioural tests use a fixed
    /// seed so they are fully reproducible; statistical tests use large sample counts with generous
    /// tolerances so they are not flaky.
    /// </summary>
    [TestFixture]
    public class RandomCSTests
    {
        // A large sample is cheap for this xorshift generator and keeps the statistical
        // tolerances comfortably away from any realistic chance of a false failure.
        private const int Samples = 200_000;

        // ---------------------------------------------------------------------
        // Seeding & determinism
        // ---------------------------------------------------------------------

        [Test]
        public void SameSeed_ProducesIdenticalSequences()
        {
            var a = new BenScr.Random.RandomCS(987654321UL);
            var b = new BenScr.Random.RandomCS(987654321UL);

            for (int i = 0; i < 1000; i++)
                Assert.That(a.NextInt(), Is.EqualTo(b.NextInt()));
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentSequences()
        {
            var a = new BenScr.Random.RandomCS(1UL);
            var b = new BenScr.Random.RandomCS(2UL);

            var seqA = new List<int>();
            var seqB = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                seqA.Add(a.NextInt());
                seqB.Add(b.NextInt());
            }

            Assert.That(seqA, Is.Not.EqualTo(seqB));
        }

        [Test]
        public void SetSeed_RestartsSequenceDeterministically()
        {
            var r = new BenScr.Random.RandomCS(42UL);

            var first = new int[5];
            for (int i = 0; i < first.Length; i++)
                first[i] = r.NextInt();

            r.SetSeed(42UL);
            for (int i = 0; i < first.Length; i++)
                Assert.That(r.NextInt(), Is.EqualTo(first[i]));
        }

        [Test]
        public void GetSeed_ReturnsAssignedSeedBeforeFirstDraw()
        {
            var r = new BenScr.Random.RandomCS(0xABCDEFUL);
            Assert.That(r.GetSeed(), Is.EqualTo(0xABCDEFUL));
        }

        [Test]
        public void DefaultConstructor_ProducesVaryingSeeds()
        {
            var seeds = new HashSet<ulong>();
            for (int i = 0; i < 16; i++)
                seeds.Add(new BenScr.Random.RandomCS().GetSeed());

            Assert.That(seeds.Count, Is.GreaterThan(1),
                "Default (auto-seeded) instances should almost never share a seed.");
        }

        [Test]
        public void Seed_Zero_DoesNotLockGeneratorIntoConstantSequence()
        {
            var r = new BenScr.Random.RandomCS(0UL);

            var values = new HashSet<int>();
            for (int i = 0; i < 100; i++)
                values.Add(r.NextInt());

            Assert.That(values.Count, Is.GreaterThan(1),
                "A zero seed should still produce a varied sequence.");
        }

        [Test]
        public void SetSeed_Zero_StoresNonZeroState()
        {
            var r = new BenScr.Random.RandomCS(12345UL);
            r.SetSeed(0UL);
            Assert.That(r.GetSeed(), Is.Not.EqualTo(0UL));
        }

        // ---------------------------------------------------------------------
        // NextInt
        // ---------------------------------------------------------------------

        [Test]
        public void NextInt_NoArgs_IsNonNegative()
        {
            var r = new BenScr.Random.RandomCS(1UL);
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextInt(), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void NextInt_WithMax_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(2UL);
            const int max = 100;
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextInt(max), Is.GreaterThanOrEqualTo(0).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithMinMax_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(3UL);
            const int min = 10, max = 20;
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextInt(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithNegativeRange_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(4UL);
            const int min = -50, max = -10;
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextInt(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [Test]
        public void NextInt_WithRange_HitsEveryValue()
        {
            var r = new BenScr.Random.RandomCS(55UL);
            var seen = new HashSet<int>();
            for (int i = 0; i < Samples; i++)
                seen.Add(r.NextInt(0, 10));

            Assert.That(seen, Is.EquivalentTo(Enumerable.Range(0, 10)));
        }

        [Test]
        public void NextInt_FullIntRange_DoesNotDegenerate()
        {
            var r = new BenScr.Random.RandomCS(123UL);
            var seen = new HashSet<int>();
            for (int i = 0; i < 1000; i++)
                seen.Add(r.NextInt(int.MinValue, int.MaxValue));

            Assert.That(seen.Count, Is.GreaterThan(1),
                "A full-width range should produce varied values.");
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void NextInt_MaxNotPositive_Throws(int max)
        {
            var r = new BenScr.Random.RandomCS(1UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextInt(max));
        }

        [TestCase(5, 5)]
        [TestCase(10, 3)]
        public void NextInt_MinNotLessThanMax_Throws(int min, int max)
        {
            var r = new BenScr.Random.RandomCS(1UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextInt(min, max));
        }

        // ---------------------------------------------------------------------
        // NextByte
        // ---------------------------------------------------------------------

        [Test]
        public void NextByte_WithMax_IsBelowMax()
        {
            var r = new BenScr.Random.RandomCS(5UL);
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextByte(100), Is.LessThan((byte)100));
        }

        [Test]
        public void NextByte_WithMinMax_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(6UL);
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextByte(20, 80), Is.GreaterThanOrEqualTo((byte)20).And.LessThan((byte)80));
        }

        [Test]
        public void NextByte_MaxZero_Throws()
        {
            var r = new BenScr.Random.RandomCS(1UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextByte(0));
        }

        [TestCase((byte)10, (byte)10)]
        [TestCase((byte)200, (byte)50)]
        public void NextByte_MinNotLessThanMax_Throws(byte min, byte max)
        {
            var r = new BenScr.Random.RandomCS(1UL);
            Assert.Throws<ArgumentOutOfRangeException>(() => r.NextByte(min, max));
        }

        // ---------------------------------------------------------------------
        // NextDouble
        // ---------------------------------------------------------------------

        [Test]
        public void NextDouble_NoArgs_IsInUnitInterval()
        {
            var r = new BenScr.Random.RandomCS(11UL);
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextDouble(), Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
        }

        [Test]
        public void NextDouble_WithMax_CoversFullRange()
        {
            var r = new BenScr.Random.RandomCS(2024UL);
            const double max = 50.0;
            double observedMax = double.MinValue;
            double sum = 0;

            for (int i = 0; i < Samples; i++)
            {
                double v = r.NextDouble(max);
                Assert.That(v, Is.GreaterThanOrEqualTo(0.0).And.LessThan(max));
                if (v > observedMax) observedMax = v;
                sum += v;
            }

            Assert.That(observedMax, Is.GreaterThan(1.0),
                "Scaled values should extend beyond 1.0 toward max.");
            Assert.That(sum / Samples, Is.EqualTo(max / 2.0).Within(max * 0.02),
                "Mean should be close to max/2 for a uniform distribution.");
        }

        [Test]
        public void NextDouble_WithMinMax_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(77UL);
            const double min = -5.0, max = 15.0;
            double sum = 0;

            for (int i = 0; i < Samples; i++)
            {
                double v = r.NextDouble(min, max);
                Assert.That(v, Is.GreaterThanOrEqualTo(min).And.LessThan(max));
                sum += v;
            }

            Assert.That(sum / Samples, Is.EqualTo((min + max) / 2.0).Within((max - min) * 0.02));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void NextDouble_MaxNotPositive_Throws(double max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BenScr.Random.RandomCS(1UL).NextDouble(max));
        }

        // ---------------------------------------------------------------------
        // NextFloat
        // ---------------------------------------------------------------------

        [Test]
        public void NextFloat_NoArgs_IsInUnitInterval()
        {
            var r = new BenScr.Random.RandomCS(12UL);
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextFloat(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void NextFloat_WithMax_CoversFullRange()
        {
            var r = new BenScr.Random.RandomCS(2025UL);
            const float max = 50f;
            float observedMax = float.MinValue;
            double sum = 0;

            for (int i = 0; i < Samples; i++)
            {
                float v = r.NextFloat(max);
                Assert.That(v, Is.GreaterThanOrEqualTo(0f).And.LessThan(max));
                if (v > observedMax) observedMax = v;
                sum += v;
            }

            Assert.That(observedMax, Is.GreaterThan(1f),
                "Scaled values should extend beyond 1.0 toward max.");
            Assert.That(sum / Samples, Is.EqualTo(max / 2.0).Within(max * 0.02));
        }

        [Test]
        public void NextFloat_WithMinMax_IsWithinRange()
        {
            var r = new BenScr.Random.RandomCS(78UL);
            const float min = 1f, max = 4f;
            for (int i = 0; i < Samples; i++)
                Assert.That(r.NextFloat(min, max), Is.GreaterThanOrEqualTo(min).And.LessThan(max));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        public void NextFloat_MaxNotPositive_Throws(float max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BenScr.Random.RandomCS(1UL).NextFloat(max));
        }

        // ---------------------------------------------------------------------
        // NextBool
        // ---------------------------------------------------------------------

        [Test]
        public void NextBool_ProducesBothOutcomes()
        {
            var r = new BenScr.Random.RandomCS(8UL);
            bool sawTrue = false, sawFalse = false;

            for (int i = 0; i < 1000 && !(sawTrue && sawFalse); i++)
            {
                if (r.NextBool()) sawTrue = true;
                else sawFalse = true;
            }

            Assert.That(sawTrue && sawFalse, Is.True);
        }

        [Test]
        public void NextBool_IsApproximatelyFair()
        {
            var r = new BenScr.Random.RandomCS(9UL);
            int trues = 0;
            for (int i = 0; i < Samples; i++)
                if (r.NextBool()) trues++;

            Assert.That((double)trues / Samples, Is.EqualTo(0.5).Within(0.02));
        }

        // ---------------------------------------------------------------------
        // NextString
        // ---------------------------------------------------------------------

        [Test]
        public void NextString_DefaultLength_IsTen()
        {
            Assert.That(new BenScr.Random.RandomCS(1UL).NextString().Length, Is.EqualTo(10));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(64)]
        public void NextString_RespectsRequestedLength(int length)
        {
            Assert.That(new BenScr.Random.RandomCS(1UL).NextString(length).Length, Is.EqualTo(length));
        }

        [Test]
        public void NextString_DefaultCharset_UsesEveryAlphanumeric()
        {
            var r = new BenScr.Random.RandomCS(2UL);
            var seen = new HashSet<char>();
            for (int i = 0; i < 50; i++)
                foreach (char c in r.NextString(1000))
                    seen.Add(c);

            var expected = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            Assert.That(seen, Is.EquivalentTo(expected));
        }

        [Test]
        public void NextString_CustomCharset_UsesOnlyProvidedCharacters()
        {
            var r = new BenScr.Random.RandomCS(3UL);
            const string charset = "01";
            string s = r.NextString(500, charset);

            Assert.That(s, Has.Length.EqualTo(500));
            Assert.That(s.All(charset.Contains), Is.True);
        }

        [Test]
        public void NextString_SingleCharacterCharset_RepeatsThatCharacter()
        {
            var r = new BenScr.Random.RandomCS(4UL);
            Assert.That(r.NextString(16, "Z"), Is.EqualTo(new string('Z', 16)));
        }

        // ---------------------------------------------------------------------
        // Generic Next<T> dispatch
        // ---------------------------------------------------------------------

        [Test]
        public void Next_DispatchesToMatchingTypedMethod()
        {
            // Each pair starts from the same seed, so the first draw of the generic method must
            // equal the first draw of the concrete method it forwards to.
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<int>(),    Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextInt()));
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<byte>(),   Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextByte()));
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<float>(),  Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextFloat()));
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<double>(), Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextDouble()));
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<bool>(),   Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextBool()));
            Assert.That(new BenScr.Random.RandomCS(100UL).Next<string>(), Is.EqualTo(new BenScr.Random.RandomCS(100UL).NextString()));
        }

        [Test]
        public void NextRange_DispatchesToMatchingTypedMethod()
        {
            Assert.That(new BenScr.Random.RandomCS(7UL).Next<int>(3, 9),          Is.EqualTo(new BenScr.Random.RandomCS(7UL).NextInt(3, 9)));
            Assert.That(new BenScr.Random.RandomCS(7UL).Next<byte>((byte)2, (byte)200), Is.EqualTo(new BenScr.Random.RandomCS(7UL).NextByte(2, 200)));
            Assert.That(new BenScr.Random.RandomCS(7UL).Next<float>(1f, 4f),      Is.EqualTo(new BenScr.Random.RandomCS(7UL).NextFloat(1f, 4f)));
            Assert.That(new BenScr.Random.RandomCS(7UL).Next<double>(1.0, 4.0),   Is.EqualTo(new BenScr.Random.RandomCS(7UL).NextDouble(1.0, 4.0)));
        }

        [Test]
        public void Next_UnsupportedType_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => new BenScr.Random.RandomCS(1UL).Next<long>());
        }

        [Test]
        public void NextRange_UnsupportedType_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => new BenScr.Random.RandomCS(1UL).Next<long>(0L, 10L));
        }
    }
}
