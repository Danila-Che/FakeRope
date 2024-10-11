using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.RigidBodyDynamics.Systems
{
	public partial class EndStepSystem : SystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private partial struct EndStepJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref FakeRigidBody rigidBody)
			{
				if (rigidBody.IsKinematic) { return; }

				rigidBody.Velocity = (rigidBody.Pose.Position - rigidBody.PreviousPose.Position) / DeltaTime;

				var deltaQuaternion = math.mul(rigidBody.Pose.Rotation, math.inverse(rigidBody.PreviousPose.Rotation));
				rigidBody.AngularVelocity = new float3(
					x: 2f * deltaQuaternion.value.x / DeltaTime,
					y: 2f * deltaQuaternion.value.y / DeltaTime,
					z: 2f * deltaQuaternion.value.z / DeltaTime);

				if (deltaQuaternion.value.w < 0f)
				{
					rigidBody.AngularVelocity = -rigidBody.AngularVelocity;
				}
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
