using FakePhysics.ECS.Utilities;

namespace FakePhysics.ECS.RigidBodyDynamics
{
	public struct FakeJoint
	{
		public FakeEntity AnchorBody;
		public FakeEntity TargetBody;
		public FakePose AnchorLocalPose;
		public FakePose TargetLocalPose;
	}
}
