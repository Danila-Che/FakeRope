using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public class StepSystem : FakeSystemBase
    {
		public float DeltaTime;

		[BurstCompile]
		private struct StepJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeParticle> Particles;
			public float DeltaTime;

			public void Execute(int index)
			{
				var particle = Particles[index];
				
				particle.Position += particle.Velocity * DeltaTime;
				
				Particles[index] = particle;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticle>();
		}

		protected override void OnUpdate()
		{
			ScheduleParallel(new StepJob
			{
				Particles = Get<FakeParticle>(),
				DeltaTime = DeltaTime,
			});
		}
	}
}
