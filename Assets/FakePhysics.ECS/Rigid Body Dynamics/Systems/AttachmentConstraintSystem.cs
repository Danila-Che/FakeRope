using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace FakePhysics.ECS.RigidBodyDynamics.Systems
{
	public class AttachmentConstraintSystem : FakeSystemBase
	{
		public float DeltaTime;

		[BurstCompile]
		private struct AttachmentConstraintJob : IJob
		{
			[ReadOnly] public FakeChunksCollection<FakeJoint> Joints;
			public FakeChunksCollection<FakeRigidBody> RigidBodies;
			public float DeltaTime;

			public void Execute()
			{
				for (int i = 0; i < Joints.Length; i++)
				{
					var joint = Joints[i];
					var anchorBody = RigidBodies[joint.AnchorBody];
					var targetBody = RigidBodies[joint.TargetBody];

					var anchorGlobalPose = Computations.Transform(anchorBody.Pose, joint.AnchorLocalPose);
					var targetGlobalPose = Computations.Transform(targetBody.Pose, joint.TargetLocalPose);

					var correction = targetGlobalPose.Position - anchorGlobalPose.Position;

					RigidBodyComputations.ApplyBodyPairCorrection(
						ref anchorBody,
						ref targetBody,
						correction,
						0.0f,
						DeltaTime,
						anchorGlobalPose.Position,
						targetGlobalPose.Position);

					RigidBodies[joint.AnchorBody] = anchorBody;
					RigidBodies[joint.TargetBody] = targetBody;
				}
			}
		}

        protected override void OnCreate()
        {
			RequireAsPrimary<FakeJoint>();
			RequireAsSecondary<FakeRigidBody>();
        }

        protected override void OnUpdate()
		{
			Schedule(new AttachmentConstraintJob
			{
				Joints = Get<FakeJoint>(),
				RigidBodies = Get<FakeRigidBody>(),
				DeltaTime = DeltaTime,
			});
		}
	}
}
