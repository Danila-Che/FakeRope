using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[SelectionBase]
	[RequireComponent(typeof(FakeBoxColliderController))]
	[DefaultExecutionOrder((int)ExecutionOrder.RigidBody)]
	public class FakeRigidBodyController : MonoBehaviour
	{
		private FakeSolverController m_FakeSolverController;
		private FakeBoxColliderController m_FakeBoxColliderController;
		private Rigidbody m_Rigidbody;

		private FakeRigidBody m_Body;

		public FakeRigidBody Body => m_Body;

		private void OnEnable()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_FakeBoxColliderController = GetComponent<FakeBoxColliderController>();

			if (m_Rigidbody == null)
			{
				m_Body = new FakeRigidBody(transform.ToPose(), m_FakeBoxColliderController.BoxCollider);
			}
			else
			{
				m_Body = new FakeRigidBody(m_Rigidbody, m_FakeBoxColliderController.BoxCollider);

				m_Rigidbody.useGravity = false;
				m_Rigidbody.detectCollisions = false;
			}

			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterRigidBody(m_Body);

				m_FakeSolverController.BeforeStep += OnBeforeStep;
			}
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

			if (m_Rigidbody == null)
			{
				transform.SetPositionAndRotation(m_Body.Pose.Position, m_Body.Pose.Rotation);
			}
			else
			{
				m_Rigidbody.position = m_Body.Pose.Position;
				m_Rigidbody.rotation = m_Body.Pose.Rotation;
			}
		}

		private void OnBeforeStep()
		{
			if (m_Body.IsKinematic)
			{
				if (m_Rigidbody == null)
				{
					m_Body.UpdateWith(transform.ToPose());
				}
				else
				{
					m_Body.UpdateWith(m_Rigidbody.ToPose());
				}
			}
		}
	}
}
