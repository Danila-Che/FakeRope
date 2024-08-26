using FakePhysics.CollisionDetection;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class CollisionComputationsTests
	{
		#region Are facing same direction cases
		[Test]
		public void Test_VectorsAreFacingSameDirection_Case0()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(0f, 1f, 0f);

			Assert.IsTrue(CollisionComputations.AreFacingSameDirection(vector0, vector1));
		}

		[Test]
		public void Test_VectorsAreFacingSameDirection_Case1()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(1f, 1f, 0f);

			Assert.IsTrue(CollisionComputations.AreFacingSameDirection(vector0, vector1));
		}

		[Test]
		public void Test_VectorsAreFacingSameDirection_Case2()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(1f, 0f, 0f);

			Assert.IsTrue(CollisionComputations.AreFacingSameDirection(vector0, vector1, checkPerpendicularity: true));
		}

		[Test]
		public void Test_VectorsAreFacingSameDirection_Case3()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(0f, -1f, 0f);

			Assert.IsFalse(CollisionComputations.AreFacingSameDirection(vector0, vector1));
		}

		#endregion

		#region Are facing different directions cases

		[Test]
		public void Test_VectorsAreFacingDifferentDirections_Case0()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(1f, 1f, 0f);

			Assert.IsFalse(CollisionComputations.AreFacingDifferentDirections(vector0, vector1));
		}

		[Test]
		public void Test_VectorsAreFacingDifferentDirections_Case1()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(0f, -1f, 0f);

			Assert.IsTrue(CollisionComputations.AreFacingDifferentDirections(vector0, vector1));
		}

		[Test]
		public void Test_VectorsAreFacingDifferentDirections_Case2()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(1f, 0f, 0f);

			Assert.IsTrue(CollisionComputations.AreFacingDifferentDirections(vector0, vector1));
		}

		[Test]
		public void Test_VectorsAreFacingDifferentDirections_Case3()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(1f, 0f, 0f);

			Assert.IsFalse(CollisionComputations.AreFacingDifferentDirections(vector0, vector1, checkPerpendicularity: true));
		}

		[Test]
		public void Test_VectorsAreFacingDifferentDirections_Case4()
		{
			var vector0 = new float3(0f, 1f, 0f);
			var vector1 = new float3(0f, 1f, 0f);

			Assert.IsFalse(CollisionComputations.AreFacingDifferentDirections(vector0, vector1));
		}

		#endregion
	}
}
