using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Jobs;

namespace FakePhysics.ECS.RigidBodyDynamics.Systems
{
	public class BeginStepSystem : FakeSystemBase
	{
		[BurstCompile]
		private struct BeginStepJob : IJobParallelFor
		{
			public FakeChunksCollection<FakeRigidBody> FakeRigidBodies;

			public void Execute(int index)
			{
				var rigidBody = FakeRigidBodies[index];

				if (rigidBody.IsKinematic) { return; }

				rigidBody.PreviousPose = rigidBody.Pose;

				FakeRigidBodies[index] = rigidBody;
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeRigidBody>();
		}

        protected override void OnUpdate()
		{
			ScheduleParallel(new BeginStepJob
			{
				FakeRigidBodies = Get<FakeRigidBody>(),
			});
		}
	}
}
