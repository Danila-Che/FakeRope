using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.RigidBodyDynamics.Systems
{
	public partial class ApplyAccelerationSystem : SystemBase
	{
		public float DeltaTime;
		public float3 Acceleration;

		[BurstCompile]
		private partial struct ApplyAccelerationJob : IJobEntity
		{
			public float DeltaTime;
			public float3 Acceleration;

			public readonly void Execute(ref TEMP_FakeRigidBody rigidBody)
			{
				if (rigidBody.IsKinematic) { return; }

				rigidBody.Velocity += Acceleration * DeltaTime;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new ApplyAccelerationJob
			{
				DeltaTime = DeltaTime,
				Acceleration = Acceleration,
			}.ScheduleParallel(Dependency);
		}
	}
}
