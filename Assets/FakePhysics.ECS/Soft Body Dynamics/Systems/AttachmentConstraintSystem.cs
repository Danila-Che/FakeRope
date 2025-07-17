using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics.Systems
{
    public class AttachmentConstraintSystem : FakeSystemBase
    {
        public float DeltaTime;

        [BurstCompile]
        private struct AttachmentConstraintJob : IJob
        {
            [ReadOnly] public FakeChunksCollection<FakeParticleJoint> Joints;
			public FakeChunksCollection<FakeParticle> Particles;
			public FakeChunksCollection<FakeRigidBody> RigidBodies;
            public float DeltaTime;

            public void Execute()
            {
                for (int i = 0; i < Joints.Length; i++)
                {
                    var joint = Joints[i];
                    var particle = Particles[joint.Particle];
                    var anchorBody = RigidBodies[joint.AnchorBody];

                    var anchorGlobalPose = Computations.Transform(anchorBody.Pose, joint.AnchorLocalPose.Position);
                    var correction = particle.Position - anchorGlobalPose;

                    if (anchorBody.IsKinematic)
                    {
                        particle.Position -= correction;
                    }
                    else if (math.any(correction))
                    {
                        SoftBodyComputations.ApplyBodyPairCorrection(
                            ref anchorBody,
                            ref particle,
                            correction,
                            compliance: 0.0f,
                            DeltaTime,
                            anchorGlobalPose);
                    }

                    Particles[joint.Particle] = particle;
                    RigidBodies[joint.AnchorBody] = anchorBody;
                }
            }
        }

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeParticleJoint>();
			RequireAsSecondary<FakeParticle>();
            RequireAsSecondary<FakeRigidBody>();
		}

        protected override void OnUpdate()
		{
			Schedule(new AttachmentConstraintJob
			{
                Joints = Get<FakeParticleJoint>(),
				Particles = Get<FakeParticle>(),
                RigidBodies = Get<FakeRigidBody>(),
				DeltaTime = DeltaTime,
			});
		}
    }
}
