using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace FakePhysics.ECS.RigidBodyDynamics.Systems
{
	public class EndStepSystem : FakeSystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private struct EndStepJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeRigidBody> RigidBodies;
			public float DeltaTime;

			public void Execute(int index)
			{
				var rigidBody = RigidBodies[index];
				
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

				RigidBodies[index] = rigidBody;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeRigidBody>();
		}

		protected override void OnUpdate()
		{
			ScheduleParallel(new EndStepJob
			{
				RigidBodies = Get<FakeRigidBody>(),
				DeltaTime = DeltaTime,
			});
		}
	}
}
