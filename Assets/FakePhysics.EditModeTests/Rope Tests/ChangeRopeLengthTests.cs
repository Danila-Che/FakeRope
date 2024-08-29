using FakePhysics.SoftBodyDynamics;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class ChangeRopeLengthTests
	{
		[Test]
		public void Test_AddParticle_CheckCount()
		{
			using var rope = new FakeRope(null, new RopeArgsBuilder().SetSpanDistance(2f));

			rope.Create(
				sourcePosition: new float3(0f, 5f, 0f),
				targetPosition: new float3(0f, 0f, 0f));

			Assert.That(rope.Particles.Length, Is.EqualTo(4));

			rope.ChangeLength(1f);

			Assert.That(rope.Particles.Length, Is.EqualTo(4));

			rope.ChangeLength(1f);

			Assert.That(rope.Particles.Length, Is.EqualTo(5));
		}

		[Test]
		public void Test_RemoveParticle_CheckCount()
		{
			using var rope = new FakeRope(null, new RopeArgsBuilder().SetSpanDistance(2f));

			rope.Create(
				sourcePosition: new float3(0f, 5f, 0f),
				targetPosition: new float3(0f, 0f, 0f));

			Assert.That(rope.Particles.Length, Is.EqualTo(4));

			rope.ChangeLength(-1f);

			Assert.That(rope.Particles.Length, Is.EqualTo(3));

			rope.ChangeLength(-1f);

			Assert.That(rope.Particles.Length, Is.EqualTo(3));
		}
	}
}
