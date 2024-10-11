using FakePhysics.ECS.SoftBodyDynamics;
using FakePhysics.ECS.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.SoftBody)]
	public class FakeRopeController : MonoBehaviour
	{
		[Header("First Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;

		[Header("Second Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;

		[Header("Settings")]
		[SerializeField] private RopeSettings m_RopeSettings;

		private FakeSolverController m_FakeSolverController;
		private FakeSoftBodySubSolver m_FakeSoftBodySubSolver;
		private FakeRopeContainer m_FakeRopeContainer;

		private float3 AnchorGlobalPose => Computations.Transform(m_AnchorBody.transform.ToPose(), m_AnchorLocalAttachement);

		private float3 TargetGlobalPose => Computations.Transform(m_TargetBody.transform.ToPose(), m_TargetLocalAttachement);

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			if (m_FakeSolverController == null)
			{
				Debug.LogError($"{nameof(FakeRigidBodyController)} must be a child of {nameof(FakeSolverController)}");
			}
			else
			{
				m_FakeSoftBodySubSolver = m_FakeSolverController.GetSubSolver<FakeSoftBodySubSolver>();
				m_FakeRopeContainer = new FakeRopeContainer(m_RopeSettings);

				m_FakeRopeContainer.Create(m_FakeSoftBodySubSolver, AnchorGlobalPose, TargetGlobalPose);

				m_FakeSolverController.StepStarting += OnStepStarting;
			}
		}

		private void OnDisable()
		{
			m_FakeSolverController.StepStarting -= OnStepStarting;
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (m_AnchorBody == null || m_TargetBody == null) { return; }

			Gizmos.color = Color.green;

			Gizmos.DrawSphere(AnchorGlobalPose, 0.1f);
			Gizmos.DrawSphere(TargetGlobalPose, 0.1f);
		}

#endif

		private void OnStepStarting()
		{
			for (int i = 0; i < m_FakeRopeContainer.Particles.Count - 1; i++)
			{
				var particle0 = m_FakeSoftBodySubSolver.Get(m_FakeRopeContainer.Particles[i]);
				var particle1 = m_FakeSoftBodySubSolver.Get(m_FakeRopeContainer.Particles[i + 1]);

 				Debug.DrawLine(particle0.Position, particle1.Position, Color.green);
			}
		}
	}
}
