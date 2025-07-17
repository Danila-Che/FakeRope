using FakePhysics.ECS.Utilities;

namespace FakePhysics.ECS.SoftBodyDynamics
{
    public struct FakeParticleJoint
    {
        public FakeEntity Particle;
        public FakeEntity AnchorBody;
        public FakePose AnchorLocalPose;
    }
}
