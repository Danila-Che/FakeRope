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

		float CalculateFirctionForceLimit(
			float frictionMagnitude,
			float3 contactNormal,
			float3 contactPoint,
			float3 deltaVDirection,
			float deltaVMagnitude);

		void ApplyCorrection(float3 correction, float3? position = null, bool velocityLevel = false);

		float3 GetVelocityAt(float3 position);
	}
}
