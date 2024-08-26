using Fake.CollisionDetection;
using Fake.EditModeTests.Utilities;
using Fake.Utilities;
using NUnit.Framework;
using Unity.Mathematics;

namespace Fake.EditModeTests
{
	[TestFixture]
	public class FakeBoxColliderTests
	{
		private Float3Comparer m_Float3Comparer;

		[SetUp]
		public void SetUp()
		{
			m_Float3Comparer = new Float3Comparer(0.0001f);
		}

		[Test]
		public void Test_BoxColliderFurthestPoint()
		{
			var boxCollider = new FakeBoxCollider(new float3(1f));
			boxCollider.Update(FakePose.k_Identity);

			var directoin = math.normalize(new float3(1f));
			var result = CollisionComputations.FindFurthestPoint(boxCollider, directoin);

			Assert.That(result, Is.EqualTo(new float3(0.5f)).Using(m_Float3Comparer));
		}
	}
}
