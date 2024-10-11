using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
	public partial class BeginStepSystem : SystemBase
	{
		[BurstCompile]
		private partial struct BeginStepJob : IJobEntity
		{
			public readonly void Execute(ref FakeParticle particle)
			{
				particle.PreviousPosition = particle.Position;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new BeginStepJob().ScheduleParallel(Dependency);
		}
	}
}
