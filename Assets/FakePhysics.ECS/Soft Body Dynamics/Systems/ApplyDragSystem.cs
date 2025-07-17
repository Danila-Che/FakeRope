using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public class ApplyDragSystem : FakeSystemBase
    {
		public float DeltaTime;

		[BurstCompile]
		private struct ApplyDragJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeParticle> Particles;
			public float DeltaTime;

			public void Execute(int index)
			{
				var particle = Particles[index];
				
				var drag = particle.Velocity * particle.Drag;
				particle.Velocity -= particle.InverseMass * DeltaTime * drag;
			
				Particles[index] = particle;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticle>();
		}

        protected override void OnUpdate()
		{
			ScheduleParallel(new ApplyDragJob
			{
				Particles = Get<FakeParticle>(),
				DeltaTime = DeltaTime,
			});
		}
	}
}
