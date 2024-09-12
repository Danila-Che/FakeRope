using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.RigidBodyDynamics.Systems
{
	public partial class ApplyDragSystem : SystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private partial struct ApplyDragJob : IJobEntity
		{
			public float DeltaTime;

			public readonly void Execute(ref TEMP_FakeRigidBody rigidBody)
			{
				if (rigidBody.IsKinematic) { return; }

				var drag = rigidBody.Velocity * rigidBody.Drag;
				rigidBody.Velocity -= rigidBody.InverseMass * DeltaTime * drag;

				var angularDrag = rigidBody.AngularVelocity * rigidBody.AngularDrag;
				rigidBody.AngularVelocity -= rigidBody.InverseMass * DeltaTime * angularDrag;
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
