using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public partial class StepSystem : SystemBase
    {
		public float DeltaTime;

		[BurstCompile]
		private partial struct StepJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref FakeParticle particle)
			{
				particle.Position += particle.Velocity * DeltaTime;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new StepJob
			{
				DeltaTime = DeltaTime,
			}.ScheduleParallel(Dependency);
		}
	}
}
