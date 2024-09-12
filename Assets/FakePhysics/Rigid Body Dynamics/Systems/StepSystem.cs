using FakePhysics.Utilities;
using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.RigidBodyDynamics.Systems
{
	public partial class StepSystem : SystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private partial struct StepJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref TEMP_FakeRigidBody rigidBody)
			{
				if (rigidBody.IsKinematic) { return; }

				rigidBody.Pose = Computations.Translate(rigidBody.Pose, rigidBody.Velocity * DeltaTime);
				rigidBody.Pose = RigidBodyComputations.ApplyRotation(rigidBody.Pose, rigidBody.AngularVelocity, DeltaTime);
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
