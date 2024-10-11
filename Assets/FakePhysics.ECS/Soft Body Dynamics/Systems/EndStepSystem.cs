using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public partial class EndStepSystem : SystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private partial struct EndStepJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref FakeParticle particle)
			{
				particle.Velocity = (particle.Position - particle.PreviousPosition) / DeltaTime;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new EndStepJob
			{
				DeltaTime = DeltaTime,
			}.ScheduleParallel(Dependency);
		}
	}
}
