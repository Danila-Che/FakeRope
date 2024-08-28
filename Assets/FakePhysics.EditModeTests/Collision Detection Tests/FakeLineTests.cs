using FakePhysics.EditModeTests.Utilities;
using FakePhysics.Utilities;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class FakeLineTests
	{
		[Test]
		public void Test_LinesAreParallel_Case0()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(1.0f, 0.0f, -1.0f), new float3(1.0f, 0.0f, 1.0f));

			Assert.IsFalse(Computations.TryGetLineIntersection(line0, line1, out _));
		}

		[Test]
		public void Test_LinesAreParallel_Case1()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, -1.0f));

			Assert.IsFalse(Computations.TryGetLineIntersection(line0, line1, out _));
		}

		[Test]
		public void Test_LineIntersectionPoint()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(-1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, 1.0f));

			Assert.IsTrue(Computations.TryGetLineIntersection(line0, line1, out var projectPoint));
			Assert.That(projectPoint, Is.EqualTo(new float3(-1.0f, 0.0f, 1.0f)));
		}

		[Test]
		public void Test_LineRotate_Stiffness_1()
		{
			var point0 = new float3(6f, 5f, 0f);
			var point1 = new float3(3f, 3f, 0f);
			var point2 = new float3(4f, 1f, 0f);

			var line0 = new FakeLine(point0, point1);
			var line1 = new FakeLine(point0, point2);

			var expected = math.normalize(point2 - point0) * math.length(point1 - point0);

			Assert.That(Computations.Rotate(line0, line1, stiffness: 1f), Is.EqualTo(expected).Using(new Float3Comparer()));
		}

		[Test]
		public void Test_LineRotate_Stiffness_0()
		{
			var point0 = new float3(6f, 5f, 0f);
			var point1 = new float3(3f, 3f, 0f);
			var point2 = new float3(4f, 1f, 0f);

			var line0 = new FakeLine(point0, point1);
			var line1 = new FakeLine(point0, point2);

			var expected = point1 - point0;

			Assert.That(Computations.Rotate(line0, line1, stiffness: 0f), Is.EqualTo(expected).Using(new Float3Comparer()));
		}
	}
}
