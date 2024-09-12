using FakePhysics.EditModeTests.Utilities;
using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using NUnit.Framework;
using Unity.Mathematics;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class RigidBodyComputationsTests
	{
		[Test]
		public void Test_ApplyRotation()
		{
			var initRotation = math.normalize(quaternion.EulerXYZ(0f, 0.1f, 0.1f));
			var deltaAngles = new float3(0f, 0.1f, -0.1f);
			var expectedRotation = math.normalize(quaternion.EulerXYZ(0f, 0.2f, 0f));

			var pose = new FakePose(float3.zero, initRotation);
			var result = RigidBodyComputations.ApplyRotation(pose, deltaAngles);

			Assert.That(
				result.Rotation,
				Is.EqualTo(expectedRotation).Using(new QuaternionComparer(1e-2f)),
				$"> {math.EulerXYZ(result.Rotation)}");
		}
	}
}
