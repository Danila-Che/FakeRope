#if USE_UNITY_ENTITIES

using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.RigidBodyDynamics.Systems
{
	public partial class BeginStepSystem : SystemBase
	{
		[BurstCompile]
		private partial struct BeginStepJob : IJobEntity
		{
			public readonly void Execute(ref TEMP_FakeRigidBody rigidBody)
			{
				if (rigidBody.IsKinematic) { return; }

				rigidBody.PreviousPose = rigidBody.Pose;
			}
		}

		protected override void OnUpdate()
		{
			Dependency = new BeginStepJob().ScheduleParallel(Dependency);
		}
	}
}

#endif
