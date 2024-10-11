using FakePhysics.ECS.Utilities;
using Unity.Entities;

namespace FakePhysics.ECS.RigidBodyDynamics
{
	[WriteGroup(typeof(FakeRigidBody))]
	public struct FakeJoint : IComponentData
	{
		public Entity AnchorBody;
		public Entity TargetBody;
		public FakePose AnchorLocalPose;
		public FakePose TargetLocalPose;
	}
}
