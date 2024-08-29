using FakePhysics.EditModeTests.Utilities;
using FakePhysics.SoftBodyDynamics;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class CreateRopeTests
	{
		private Float3Comparer m_Float3Comparer;

		[SetUp]
		public void SetUp()
		{
			m_Float3Comparer = new Float3Comparer(0.0001f);
		}

		[Test]
		public void Test_Create_Case0()
		{
			using var rope = new FakeRope(null, new RopeArgsBuilder().SetSpanDistance(2f));
			var sourcePosition = new float3(0f, 5f, 0f);
			var targetPosition = new float3(0f, 0f, 0f);

			rope.Create(sourcePosition, targetPosition);

			Assert.That(rope.Particles.Length, Is.EqualTo(4));
			Assert.That(rope.Particles[0].Position, Is.EqualTo(targetPosition).Using(m_Float3Comparer));
			Assert.That(rope.Particles[1].Position, Is.EqualTo(new float3(0f, 2f, 0f)).Using(m_Float3Comparer));
			Assert.That(rope.Particles[2].Position, Is.EqualTo(new float3(0f, 4f, 0f)).Using(m_Float3Comparer));
			Assert.That(rope.Particles[3].Position, Is.EqualTo(sourcePosition).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_Create_Case1()
		{
			using var rope = new FakeRope(null, new RopeArgsBuilder().SetSpanDistance(2f));
			var sourcePosition = new float3(0f, 4f, 0f);
			var targetPosition = new float3(0f, 0f, 0f);

			rope.Create(sourcePosition, targetPosition);

			Assert.That(rope.Particles.Length, Is.EqualTo(3));
			Assert.That(rope.Particles[0].Position, Is.EqualTo(targetPosition).Using(m_Float3Comparer));
			Assert.That(rope.Particles[1].Position, Is.EqualTo(new float3(0f, 2f, 0f)).Using(m_Float3Comparer));
			Assert.That(rope.Particles[2].Position, Is.EqualTo(sourcePosition).Using(m_Float3Comparer));
		}
	}
}
