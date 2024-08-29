using FakePhysics.EditModeTests.Utilities;
using FakePhysics.SoftBodyDynamics;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class ConstraintProjectionTests
	{
		[Test]
		public void Test_DistanceConstraintFunction()
		{
			var p1 = new float3(0f, 1.5f, 0f);
			var p2 = new float3(0f, 0f, 0f);
			var distance = 1f;

			var C_dis = 1.5f - distance;

			Assert.That(SoftBodyComputations.CalculateDistanceConstraint(p1, p2, distance), Is.EqualTo(C_dis));
		}

		[Test]
		public void Test_DistanceConstraintGradient()
		{
			var p1 = new float3(2f, 2f, 0f);
			var p2 = new float3(0f, 0f, 0f);

			var n = (p1 - p2) / math.length(p1 - p2);

			Assert.That(SoftBodyComputations.CalculateDistanceGradient(p1, p2), Is.EqualTo(n).Using(new Float3Comparer()));
		}

		[Test]
		public void Test_BendConstraintFunction()
		{
			var p1 = new float3(2f, 2f, 0f);
			var p2 = new float3(0f, 0f, 0f);
			var p3 = new float3(0f, -2f, 0f);

			var C_bend = math.radians(135);

			Assert.That(SoftBodyComputations.CalculateBendConstraint(p1, p2, p3, 0f), Is.EqualTo(C_bend));
		}

		[Test]
		public void Test_BendConstraintGradient()
		{
			var p1 = new float3(1f, -1f, 0f);
			var p2 = new float3(-1f, 0f, 0f);
			var p3 = new float3(1f, 1f, 0f);

			var n = new float3(-4f / 3f, 0f, 0f);

			Assert.That(SoftBodyComputations.CalculateBendGradient(p1, p2, p3), Is.EqualTo(n).Using(new Float3Comparer()));
		}
	}
}
