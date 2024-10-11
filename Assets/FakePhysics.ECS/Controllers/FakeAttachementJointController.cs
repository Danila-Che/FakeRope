using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.Joint)]
	public class FakeAttachementJointController : MonoBehaviour
	{
		[Header("First Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;

		[Header("Second Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;

		private FakeSolverController m_FakeSolverController;
		private FakeRigidBodySubSolver m_FakeRigidBodySubSolver;

		private Entity m_JointEntity;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			if (m_FakeSolverController == null)
			{
				Debug.LogError($"{nameof(FakeRigidBodyController)} must be a child of {nameof(FakeSolverController)}");
			}
			else
			{
				m_FakeRigidBodySubSolver = m_FakeSolverController.GetSubSolver<FakeRigidBodySubSolver>();

				var joint = new FakeJoint()
				{
					AnchorBody = m_AnchorBody.RigidBodyEntity,
					TargetBody = m_TargetBody.RigidBodyEntity,
					AnchorLocalPose = m_AnchorLocalAttachement,
					TargetLocalPose = m_TargetLocalAttachement,
				};

				m_JointEntity = m_FakeRigidBodySubSolver.RequireEntity(joint);
			}
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (m_AnchorBody == null || m_TargetBody == null) { return; }

			Gizmos.color = Color.green;

			var anchorGlobalPose = Computations.Transform(m_AnchorBody.transform.ToPose(), m_AnchorLocalAttachement);
			var targetGlobalPose = Computations.Transform(m_TargetBody.transform.ToPose(), m_TargetLocalAttachement);

			Gizmos.DrawSphere(anchorGlobalPose, 0.1f);
			Gizmos.DrawSphere(targetGlobalPose, 0.1f);
		}

#endif
	}
}
