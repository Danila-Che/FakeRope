using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public partial class ApplyDragSystem : SystemBase
    {
		public float DeltaTime;

		[BurstCompile]
		private partial struct ApplyDragJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref FakeParticle particle)
			{
				var drag = particle.Velocity * particle.Drag;
				particle.Velocity -= particle.InverseMass * DeltaTime * drag;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new ApplyDragJob
			{
				DeltaTime = DeltaTime,
			}.ScheduleParallel(Dependency);
		}
	}
}
