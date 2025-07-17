using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
	public class BeginStepSystem : FakeSystemBase
	{
		[BurstCompile]
		private struct BeginStepJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeParticle> Particles;

			public void Execute(int index)
			{
				var particle = Particles[index];

				particle.PreviousPosition = particle.Position;

				Particles[index] = particle;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticle>();
		}

        protected override void OnUpdate()
		{
			ScheduleParallel(new BeginStepJob
			{
				Particles = Get<FakeParticle>(),
			});
		}
	}
}
