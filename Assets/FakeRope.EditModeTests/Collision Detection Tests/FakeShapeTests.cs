using Fake.CollisionDetection;
using NUnit.Framework;
using Unity.Mathematics;

namespace Fake.EditModeTests
{
	[TestFixture]
	public class FakeShapeTests
	{
		[Test]
		public void Test_PointInsideShape()
		{
			var normal = math.up();
			var shape = new FakeShape(4);
			shape.Add(new float3(-1.0f, 0.0f, -1.0f));
			shape.Add(new float3(-1.0f, 0.0f, 1.0f));
			shape.Add(new float3(1.0f, 0.0f, -1.0f));
			shape.Add(new float3(1.0f, 0.0f, 1.0f));

			CollisionComputations.CalculateAngles(shape, normal);
			shape.Sort(CollisionComputations.CompareAngle);

			var point = float3.zero;

			Assert.IsTrue(CollisionComputations.IsInside(shape, point, normal));
		}

		[Test]
		public void Test_PointInsideShape_PointOnShape()
		{
			var normal = math.up();
			var shape = new FakeShape(4);
			shape.Add(new float3(-1.0f, 0.0f, -1.0f));
			shape.Add(new float3(-1.0f, 0.0f, 1.0f));
			shape.Add(new float3(1.0f, 0.0f, -1.0f));
			shape.Add(new float3(1.0f, 0.0f, 1.0f));

			CollisionComputations.CalculateAngles(shape, normal);
			shape.Sort(CollisionComputations.CompareAngle);

			var point = new float3(1.0f, 0.0f, 0.5f);

			Assert.IsTrue(CollisionComputations.IsInside(shape, point, normal, checkOnShape: true));
		}

		[Test]
		public void Test_CheckOutsideShape()
		{
			var normal = math.up();
			var shape = new FakeShape(4);
			shape.Add(new float3(-1.0f, 0.0f, -1.0f));
			shape.Add(new float3(-1.0f, 0.0f, 1.0f));
			shape.Add(new float3(1.0f, 0.0f, -1.0f));
			shape.Add(new float3(1.0f, 0.0f, 1.0f));

			CollisionComputations.CalculateAngles(shape, normal);
			shape.Sort(CollisionComputations.CompareAngle);

			var point = 2.0f * math.right();

			Assert.IsFalse(CollisionComputations.IsInside(shape, point, normal));
		}
	}
}
