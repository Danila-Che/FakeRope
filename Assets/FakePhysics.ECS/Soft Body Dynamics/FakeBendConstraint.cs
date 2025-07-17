using FakePhysics.ECS.Utilities;

namespace FakePhysics.ECS.SoftBodyDynamics
{
    public struct FakeBendConstraint
    {
        public FakeEntity Particle0;
		public FakeEntity Particle1;
		public FakeEntity Particle2;
		public float Stiffness;
    }
}
