using Fake.RigidBodyDynamics;
using Fake.Utilities;
using UnityEngine;

namespace Fake.Controllers
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	[DefaultExecutionOrder((int)ExecutionOrder.RigidBody)]
	internal class FakeRigidBodyController : MonoBehaviour
	{
		private FakeSolverController m_FakeSolverController;
		private Rigidbody m_Rigidbody;

		private FakeRigidBody m_Body;

		public FakeRigidBody Body => m_Body;

		private void OnEnable()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			var boxColliderController = GetComponent<FakeBoxColliderController>();
			m_Body = new FakeRigidBody(m_Rigidbody, boxColliderController.BoxCollider);

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterRigidBody(m_Body);

				m_FakeSolverController.BeforeStep += OnBeforeStep;
			}

			m_Rigidbody.useGravity = false;
			m_Rigidbody.detectCollisions = false;
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterRigidBody(m_Body);

				m_FakeSolverController.BeforeStep -= OnBeforeStep;
			}
		}

		private void FixedUpdate()
		{
			if (m_Body.IsKinematic) { return; }

			m_Rigidbody.position = m_Body.Pose.Position;
			m_Rigidbody.rotation = m_Body.Pose.Rotation;
		}

		private void OnBeforeStep()
		{
			if (m_Body.IsKinematic)
			{
				m_Body.UpdateWith(m_Rigidbody.ToPose());
			}
		}
	}
}
