using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public struct FakeParticle
	{
		public float3 PreviousPosition;
		public float3 Position;
		public float3 Velocity;
		public float InverseMass;

		public FakeParticle(float3 position, float mass)
		{
			PreviousPosition = position;
			Position = position;
			Velocity = float3.zero;
			InverseMass = 1.0f / mass;
		}
	}
}
