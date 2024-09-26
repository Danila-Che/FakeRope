using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.Joint)]
	public class FakeAttachementJointController : MonoBehaviour
	{
		[Header("First Body")]
		[SerializeField] private FakeRigidBodyController m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;

		[Header("Second Body")]
		[SerializeField] private FakeRigidBodyController m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;

		private FakeSolverController m_FakeSolverController;
		private FakeAttachmentConstraint m_FakeAttachmentConstraint;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			m_FakeAttachmentConstraint = new FakeAttachmentConstraint(CreateJoint());

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterSolfBody(m_FakeAttachmentConstraint);
			}
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterSolfBody(m_FakeAttachmentConstraint);
			}
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (m_AnchorBody == null || m_TargetBody == null) { return; }

			var joint = CreateJoint();

			Gizmos.color = Color.green;

			joint.RecalculateGlobalPoses(m_AnchorBody.transform, m_TargetBody.transform);

			Gizmos.DrawSphere(joint.AnchorGlobalPose.Position, 0.1f);
			Gizmos.DrawSphere(joint.TargetGlobalPose.Position, 0.1f);
		}

#endif

		private FakeJoint CreateJoint()
		{
			return new FakeJoint(
				m_AnchorBody.Body,
				m_TargetBody.Body,
				m_AnchorLocalAttachement,
				m_TargetLocalAttachement);
		}
	}
}
