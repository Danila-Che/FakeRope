using FakePhysics.CollisionDetection;
using FakePhysics.Utilities;
using Unity.Mathematics;

namespace FakePhysics.Dynamics
{
	public interface ICollidingBody
	{
		bool IsKinematic { get; }

		FakePose Pose { get; }

		FakeBoxCollider BoxCollider { get; }

		float GetInverseMass(float3 normal, float3? position = null);

		void ApplyCorrection(float3 correction, float3? position = null, bool velocityLevel = false);
	}
}
