using FakePhysics.Dynamics;
using FakePhysics.Utilities;

namespace FakePhysics.RigidBodyDynamics
{
	public class FakeAttachmentConstraint : IConstrainedBody
	{
		private readonly FakeJoint m_FakeJoint;

		public FakeAttachmentConstraint(FakeJoint fakeJoint)
		{
			m_FakeJoint = fakeJoint;
		}

		public void SolveInnerConstraints(float deltaTime) { }

		public void SolveOuterConstraints(float deltaTime)
		{
			m_FakeJoint.RecalculateGlobalPoses();

			var correction = Computations.CalculateCorrection(m_FakeJoint.TargetGlobalPose.Position, m_FakeJoint.AnchorGlobalPose.Position);

			DynamicsComputations.ApplyBodyPairCorrection(
				m_FakeJoint.AnchorBody,
				m_FakeJoint.TargetBody,
				correction,
				0f,
				deltaTime,
				m_FakeJoint.AnchorGlobalPose.Position,
				m_FakeJoint.TargetGlobalPose.Position);
		}
	}
}
