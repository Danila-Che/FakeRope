using FakePhysics.EditModeTests.Utilities;
using FakePhysics.SoftBodyDynamics;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
    [TestFixture]
    public class ChangeRopeLengthTests
    {
		private Float3Comparer m_Float3Comparer;

		[SetUp]
		public void SetUp()
		{
			m_Float3Comparer = new Float3Comparer(0.0001f);
		}

		[Test]
        public void Test_AddParticle_CheckCount()
        {
            using var rope = new FakeRope(null, new RopeArgs(spanDistance: 2f));

            rope.Create(
                sourcePosition: new float3(0f, 5f, 0f),
                targetPosition: new float3(0f, 0f, 0f));

            Assert.That(rope.Particles.Count, Is.EqualTo(4));

            rope.ChangeLength(1f);

			Assert.That(rope.Particles.Count, Is.EqualTo(4));

            rope.ChangeLength(1f);

			Assert.That(rope.Particles.Count, Is.EqualTo(5));
		}

		[Test]
		public void Test_RemoveParticle_CheckCount()
		{
			using var rope = new FakeRope(null, new RopeArgs(spanDistance: 2f));

			rope.Create(
				sourcePosition: new float3(0f, 5f, 0f),
				targetPosition: new float3(0f, 0f, 0f));

			Assert.That(rope.Particles.Count, Is.EqualTo(4));

			rope.ChangeLength(-1f);

			Assert.That(rope.Particles.Count, Is.EqualTo(3));

			rope.ChangeLength(-1f);

			Assert.That(rope.Particles.Count, Is.EqualTo(3));
		}
	}
}
