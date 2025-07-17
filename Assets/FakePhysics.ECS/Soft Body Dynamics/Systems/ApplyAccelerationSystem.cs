using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
	public class ApplyAccelerationSystem : FakeSystemBase
	{
		public float3 Acceleration;
		public float DeltaTime;

		[BurstCompile]
		private struct ApplyAccelerationJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeParticle> Particles;
			public float3 Acceleration;
			public float DeltaTime;

			public void Execute(int index)
			{
				var particle = Particles[index];
				
				particle.Velocity += Acceleration * DeltaTime;
				
				Particles[index] = particle;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticle>();
		}

		protected override void OnUpdate()
		{
			ScheduleParallel(new ApplyAccelerationJob
			{
				Particles = Get<FakeParticle>(),
				Acceleration = Acceleration,
				DeltaTime = DeltaTime,
			});
		}
	}
}
