using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Entities;

namespace FakePhysics.ECS.RigidBodyDynamics.Systems
{
	public partial class AttachmentConstraintSystem : SystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private partial struct AttachmentConstraintJob : IJobEntity
		{
			public EntityManager EntityManager;
			public float DeltaTime;

			public readonly void Execute(in FakeJoint fakeJoint)
			{
				var anchorBody = EntityManager.GetComponentData<FakeRigidBody>(fakeJoint.AnchorBody);
				var targetBody = EntityManager.GetComponentData<FakeRigidBody>(fakeJoint.TargetBody);

				var anchorGlobalPose = Computations.Transform(anchorBody.Pose, fakeJoint.AnchorLocalPose);
				var targetGlobalPose = Computations.Transform(targetBody.Pose, fakeJoint.TargetLocalPose);

				var correction = targetGlobalPose.Position - anchorGlobalPose.Position;

				RigidBodyComputations.ApplyBodyPairCorrection(
					ref anchorBody,
					ref targetBody,
					correction,
					0.0f,
					DeltaTime,
					anchorGlobalPose.Position,
					targetGlobalPose.Position);

				EntityManager.SetComponentData(fakeJoint.AnchorBody, anchorBody);
				EntityManager.SetComponentData(fakeJoint.TargetBody, targetBody);
			}
		}

		protected override void OnUpdate()
		{
			var entityManager = World.EntityManager;

			new AttachmentConstraintJob
			{
				EntityManager = entityManager,
				DeltaTime = DeltaTime,
			}.Run();
		}
	}
}
