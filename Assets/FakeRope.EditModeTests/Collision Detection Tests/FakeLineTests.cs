using Fake.CollisionDetection;
using NUnit.Framework;
using Unity.Mathematics;

namespace Fake.EditModeTests
{
	[TestFixture]
	public class FakeLineTests
	{
		[Test]
		public void Test_LinesAreParallel_Case0()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(1.0f, 0.0f, -1.0f), new float3(1.0f, 0.0f, 1.0f));

			Assert.IsFalse(CollisionComputations.TryGetLineIntersection(line0, line1, out _));
		}

		[Test]
		public void Test_LinesAreParallel_Case1()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, -1.0f));

			Assert.IsFalse(CollisionComputations.TryGetLineIntersection(line0, line1, out _));
		}

		[Test]
		public void Test_LineIntersectionPoint()
		{
			var line0 = new FakeLine(new float3(-1.0f, 0.0f, -1.0f), new float3(-1.0f, 0.0f, 1.0f));
			var line1 = new FakeLine(new float3(-1.0f, 0.0f, 1.0f), new float3(1.0f, 0.0f, 1.0f));

			Assert.IsTrue(CollisionComputations.TryGetLineIntersection(line0, line1, out var projectPoint));
			Assert.That(projectPoint, Is.EqualTo(new float3(-1.0f, 0.0f, 1.0f)));
		}
	}
}
