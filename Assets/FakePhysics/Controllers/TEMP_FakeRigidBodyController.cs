using FakePhysics.Utilities;
using Unity.Entities;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(FakeBoxColliderController))]
	[DefaultExecutionOrder((int)ExecutionOrder.RigidBody)]
	public class TEMP_FakeRigidBodyController : MonoBehaviour
	{
		private TEMP_FakeSolverController m_FakeSolverController;
		private Rigidbody m_Rigidbody;

		private Entity m_Entity;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<TEMP_FakeSolverController>();
			m_Rigidbody = GetComponent<Rigidbody>();
			var boxCollider = GetComponent<FakeBoxColliderController>();

			m_Entity = m_FakeSolverController.RequireEnity(m_Rigidbody, boxCollider.BoxCollider);
		}

		private void FixedUpdate()
		{
			var rigidBody = m_FakeSolverController.Get(m_Entity);

			if (rigidBody.IsKinematic)
			{
				rigidBody.Pose = m_Rigidbody.ToPose();

				m_FakeSolverController.Set(m_Entity, rigidBody);
			}
			else
			{
				m_Rigidbody.position = rigidBody.Pose.Position;
				m_Rigidbody.rotation = rigidBody.Pose.Rotation;
			}
		}
	}
}
