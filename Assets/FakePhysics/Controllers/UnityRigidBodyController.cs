using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(FakeBoxColliderController))]
	[DefaultExecutionOrder((int)ExecutionOrder.RigidBody)]
	public class UnityRigidBodyController : FakeRigidBodyControllerBase
	{
		private FakeSolverController m_FakeSolverController;
		private FakeBoxColliderController m_FakeBoxColliderController;
		private Rigidbody m_Rigidbody;

		private FakeRigidBody m_Body;

		public override FakeRigidBody Body => m_Body;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_FakeBoxColliderController = GetComponent<FakeBoxColliderController>();

			m_Body = new FakeRigidBody(m_Rigidbody, m_FakeBoxColliderController.BoxCollider);

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.BeforeStep += OnBeginStep;
				m_FakeSolverController.AfterStep += OnAfterStep;
			}
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.BeforeStep -= OnBeginStep;
				m_FakeSolverController.AfterStep -= OnAfterStep;
			}
		}

		private void OnBeginStep()
		{
			m_Body.UpdateWith(m_Rigidbody);
			m_Body.BeginStep();
		}

		private void OnAfterStep()
		{
			if (m_Body.IsKinematic) { return; }

			m_Body.EndStep(Time.fixedDeltaTime);

			m_Rigidbody.position = m_Body.Pose.Position;
			m_Rigidbody.rotation = m_Body.Pose.Rotation;
			m_Rigidbody.velocity += (Vector3)m_Body.Velocity;
			m_Rigidbody.angularVelocity += (Vector3)m_Body.AngularVelocity;
		}
	}
}
