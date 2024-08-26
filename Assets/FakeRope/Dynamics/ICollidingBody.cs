using Fake.CollisionDetection;
using Fake.Utilities;
using Unity.Mathematics;

namespace Fake.Dynamics
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
