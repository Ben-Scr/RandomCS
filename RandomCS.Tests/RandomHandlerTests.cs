using System.Collections.Generic;
using System.Linq;
using BenScr.Random;

namespace RandomCS.Tests
{
    /// <summary>
    /// Tests for the static <see cref="RandomHandler"/> facade and its nested
    /// <see cref="RandomHandler.Secure"/> class. The non-secure side is backed by a shared,
    /// seedable instance, so seed-dependent tests set the seed themselves to stay independent
    /// of test execution order.
    /// </summary>
    [TestFixture]
    public class RandomHandlerTests
    {
        private const int Samples = 50_000;

        // ---------------------------------------------------------------------
        // Seeding (non-secure)
        // ---------------------------------------------------------------------

        [Test]
        public void SetSeed_MakesSequenceReproducible()
        {
            RandomHandler.SetSeed(2024UL);
            var first = new int[10];
            for (int i = 0; i < first.Length; i++)
                first[i] = RandomHandler.NextInt(0, 1000);

            RandomHandler.SetSeed(2024UL);
            for (int i = 0; i < first.Length; i++)
                Assert.That(RandomHandler.NextInt(0, 1000), Is.EqualTo(first[i]));
        }

        [Test]
        public void SetSeed_MatchesEquivalentRandomCSInstance()
        {
            RandomHandler.SetSeed(99UL);
            var reference = new BenScr.Random.RandomCS(99UL);

            for (int i = 0; i < 100; i++)
                Assert.That(RandomHandler.NextInt(0, 10000), Is.EqualTo(reference.NextInt(0, 10000)));
        }

        [Test]
        public void RemoveSeed_DoesNotThrowAndKeepsGenerating()
        {
            RandomHandler.SetSeed(1UL);
            Assert.DoesNotThrow(() => RandomHandler.RemoveSeed());
            Assert.DoesNotThrow(() => RandomHandler.NextInt(0, 10));
        }

        // ---------------------------------------------------------------------
        // Non-secure surface
        // ---------------------------------------------------------------------

        [Test]
        public void NextInt_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.NextInt(5, 15), Is.GreaterThanOrEqualTo(5).And.LessThan(15));
        }

        [Test]
        public void NextFloat_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.NextFloat(0f, 10f), Is.GreaterThanOrEqualTo(0f).And.LessThan(10f));
        }

        [Test]
        public void NextDouble_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.NextDouble(0.0, 10.0), Is.GreaterThanOrEqualTo(0.0).And.LessThan(10.0));
        }

        [Test]
        public void NextBool_ProducesBothOutcomes()
        {
            RandomHandler.SetSeed(8UL);
            bool sawTrue = false, sawFalse = false;
            for (int i = 0; i < 1000 && !(sawTrue && sawFalse); i++)
            {
                if (RandomHandler.NextBool()) sawTrue = true;
                else sawFalse = true;
            }
            Assert.That(sawTrue && sawFalse, Is.True);
        }

        [Test]
        public void NextByte_ProducesVariety()
        {
            RandomHandler.SetSeed(7UL);
            var seen = new HashSet<byte>();
            for (int i = 0; i < 5000; i++)
                seen.Add(RandomHandler.NextByte());

            Assert.That(seen.Count, Is.GreaterThan(1));
        }

        [Test]
        public void NextString_DefaultLength_IsTen()
        {
            Assert.That(RandomHandler.NextString().Length, Is.EqualTo(10));
        }

        [Test]
        public void NextString_CustomLengthAndCharset_AreRespected()
        {
            const string charset = "AB";
            string s = RandomHandler.NextString(32, charset);
            Assert.That(s, Has.Length.EqualTo(32));
            Assert.That(s.All(charset.Contains), Is.True);
        }

        [Test]
        public void Next_Generic_ReturnsValueConsistentWithType()
        {
            Assert.That(RandomHandler.Next<int>(), Is.GreaterThanOrEqualTo(0));
            Assert.That(RandomHandler.Next<string>(), Has.Length.EqualTo(10));
            Assert.DoesNotThrow(() => RandomHandler.Next<bool>());
        }

        [Test]
        public void NextRange_Generic_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.Next<int>(3, 9), Is.GreaterThanOrEqualTo(3).And.LessThan(9));
        }

        // ---------------------------------------------------------------------
        // Secure surface
        // ---------------------------------------------------------------------

        [Test]
        public void Secure_NextInt_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.Secure.NextInt(5, 15), Is.GreaterThanOrEqualTo(5).And.LessThan(15));
        }

        [Test]
        public void Secure_NextFloat_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.Secure.NextFloat(0f, 10f), Is.GreaterThanOrEqualTo(0f).And.LessThan(10f));
        }

        [Test]
        public void Secure_NextDouble_WithMinMax_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.Secure.NextDouble(0.0, 10.0), Is.GreaterThanOrEqualTo(0.0).And.LessThan(10.0));
        }

        [Test]
        public void Secure_NextByte_ProducesVariety()
        {
            var seen = new HashSet<byte>();
            for (int i = 0; i < 5000; i++)
                seen.Add(RandomHandler.Secure.NextByte());

            Assert.That(seen.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Secure_NextBool_ProducesBothOutcomes()
        {
            bool sawTrue = false, sawFalse = false;
            for (int i = 0; i < 1000 && !(sawTrue && sawFalse); i++)
            {
                if (RandomHandler.Secure.NextBool()) sawTrue = true;
                else sawFalse = true;
            }
            Assert.That(sawTrue && sawFalse, Is.True);
        }

        [Test]
        public void Secure_NextString_DefaultLength_IsTen()
        {
            Assert.That(RandomHandler.Secure.NextString().Length, Is.EqualTo(10));
        }

        [Test]
        public void Secure_GenerateBytes_FillsBuffer()
        {
            var buffer = new byte[256];
            RandomHandler.Secure.GenerateBytes(buffer);
            Assert.That(buffer.Any(b => b != 0), Is.True);
        }

        [Test]
        public void Secure_Next_Generic_ReturnsValueConsistentWithType()
        {
            Assert.That(RandomHandler.Secure.Next<int>(), Is.GreaterThanOrEqualTo(0));
            Assert.That(RandomHandler.Secure.Next<string>(), Has.Length.EqualTo(10));
        }

        [Test]
        public void Secure_NextRange_Generic_IsWithinRange()
        {
            for (int i = 0; i < Samples; i++)
                Assert.That(RandomHandler.Secure.Next<int>(3, 9), Is.GreaterThanOrEqualTo(3).And.LessThan(9));
        }
    }
}
