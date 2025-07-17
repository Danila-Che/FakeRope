using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public class EndStepSystem : FakeSystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private struct EndStepJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeParticle> Particles;
			public float DeltaTime;

			public void Execute(int index)
			{
				var particle = Particles[index];
				
				particle.Velocity = (particle.Position - particle.PreviousPosition) / DeltaTime;
			
				Particles[index] = particle;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticle>();
		}

		protected override void OnUpdate()
		{
			ScheduleParallel(new EndStepJob
			{
				Particles = Get<FakeParticle>(),
				DeltaTime = DeltaTime,
			});
		}
	}
}
