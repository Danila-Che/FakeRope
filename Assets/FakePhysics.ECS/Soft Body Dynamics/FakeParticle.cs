using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public struct FakeParticle : IComponentData
	{
		public float3 PreviousPosition;
		public float3 Position;
		public float3 Velocity;
		public float Drag;
		public float InverseMass;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakeParticle(float3 position, float mass, float drag)
		{
			PreviousPosition = position;
			Position = position;
			Velocity = float3.zero;
			Drag = drag;
			InverseMass = 1.0f / mass;
		}
	}
}
