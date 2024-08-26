using FakePhysics.RigidBodyDynamics;
using FakePhysics.SoftBodyDynamics;
using FakePhysics.SoftBodyDynamics.Renderer;
using FakePhysics.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[RequireComponent(typeof(IRenderer))]
	[DefaultExecutionOrder((int)ExecutionOrder.SoftBody)]
	public class FakeRopeController : MonoBehaviour
	{
		[Header("First Body")]
		[SerializeField] private FakeRigidBodyController m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;

		[Header("Second Body")]
		[SerializeField] private FakeRigidBodyController m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;

		[Header("Settings")]
		[SerializeField] private RopeArgs m_RopeArgs;

		private FakeRope m_Rope;
		private IRenderer m_Renderer;

		private FakeSolverController m_FakeSolverController;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			m_Rope = new FakeRope(CreateJoint(), m_RopeArgs);
			m_Rope.CreateFromJoint();

			m_Renderer = GetComponent<IRenderer>();
			m_Renderer.Init();

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterSolfBody(m_Rope);
			}
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterSolfBody(m_Rope);
			}
		}

		private void Update()
		{
			m_Renderer.Draw(m_Rope.Particles);
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

			m_Rope?.Particles.ForEach(particle => Gizmos.DrawSphere(particle.Position, 0.05f));
		}

#endif

		private FakeJoint CreateJoint()
		{
			return new FakeJoint(
				m_AnchorBody.Body,
				m_TargetBody.Body,
				new FakePose(m_AnchorLocalAttachement, quaternion.identity),
				new FakePose(m_TargetLocalAttachement, quaternion.identity));
		}
	}
}
