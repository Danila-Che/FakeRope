using FakePhysics.Utilities;
using Unity.Mathematics;

namespace FakePhysics.RigidBodyDynamics
{
	public class FakeJoint
	{
		private readonly FakeRigidBody m_AnchorBody;
		private readonly FakeRigidBody m_TargetBody;
		private readonly FakePose m_AnchorLocalPose;
		private readonly FakePose m_TargetLocalPose;

		private FakePose m_AnchorGlobalPose;
		private FakePose m_TargetGlobalPose;

		public FakeJoint(
			FakeRigidBody anchorBody,
			FakeRigidBody targetBody,
			FakePose anchorLocalPose,
			FakePose targetLocalPose)
		{
			m_AnchorBody = anchorBody;
			m_TargetBody = targetBody;
			m_AnchorLocalPose = anchorLocalPose;
			m_TargetLocalPose = targetLocalPose;
		}

		public bool IsValid => m_AnchorBody != null && m_TargetBody != null;

		public FakeRigidBody AnchorBody => m_AnchorBody;
		public FakeRigidBody TargetBody => m_TargetBody;

		public FakePose AnchorGlobalPose => m_AnchorGlobalPose;
		public FakePose TargetGlobalPose => m_TargetGlobalPose;

		public void RecalculateGlobalPoses()
		{
			m_AnchorGlobalPose = m_AnchorBody == null ? m_AnchorLocalPose : Computations.Transform(m_AnchorBody.Pose, m_AnchorLocalPose);
			m_TargetGlobalPose = m_TargetBody == null ? m_TargetLocalPose : Computations.Transform(m_TargetBody.Pose, m_TargetLocalPose);
		}

		public void RecalculateGlobalPoses(UnityEngine.Transform anchor, UnityEngine.Transform target)
		{
			m_AnchorGlobalPose = Computations.Transform(new FakePose(anchor.position, anchor.rotation), m_AnchorLocalPose);
			m_TargetGlobalPose = Computations.Transform(new FakePose(target.position, target.rotation), m_TargetLocalPose);
		}
	}
}
