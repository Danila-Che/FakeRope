using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
	public partial class ApplyAccelerationSystem : SystemBase
	{
		public float3 Acceleration;
		public float DeltaTime;

		[BurstCompile]
		private partial struct ApplyAccelerationJob : IJobEntity
		{
			public float3 Acceleration;
			public float DeltaTime;

			public readonly void Execute(ref FakeParticle particle)
			{
				particle.Velocity += Acceleration * DeltaTime;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new ApplyAccelerationJob
			{
				Acceleration = Acceleration,
				DeltaTime = DeltaTime,
			}.ScheduleParallel(Dependency);
		}
	}
}
