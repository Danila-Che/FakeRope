using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	[RequireComponent(typeof(Rigidbody))]
	[DefaultExecutionOrder((int)ExecutionOrder.RigidBody)]
	public class FakeRigidBodyController : FakeRigidBodyControllerBase
	{
		private Rigidbody m_Rigidbody;
		private FakeSolverController m_FakeSolverController;
		private FakeRigidBodySubSolver m_FakeRigidBodySubSolver;

		private Entity m_RigidBodyEntity;

		public override Entity RigidBodyEntity => m_RigidBodyEntity;

		private void OnEnable()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			if (m_FakeSolverController == null)
			{
				Debug.LogError($"{nameof(FakeRigidBodyController)} must be a child of {nameof(FakeSolverController)}");
			}
			else
			{
				m_FakeRigidBodySubSolver = m_FakeSolverController.GetSubSolver<FakeRigidBodySubSolver>();
				m_RigidBodyEntity = m_FakeRigidBodySubSolver.RequireEntity(CreateFakeRigidBody());

				m_FakeSolverController.Step += OnStep;
				m_FakeSolverController.StepCompleted += OnStepCompleted;
			}
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.Step -= OnStep;
				m_FakeSolverController.StepCompleted -= OnStepCompleted;
			}
		}

		private void OnStep()
		{
			var rigidBody = m_FakeRigidBodySubSolver.Get(m_RigidBodyEntity);

			m_FakeRigidBodySubSolver.Set(m_RigidBodyEntity, UpdateFakeRigidBody(rigidBody));
		}

		private void OnStepCompleted()
		{
			if (m_Rigidbody.isKinematic) { return; }

			var rigidBody = m_FakeRigidBodySubSolver.Get(m_RigidBodyEntity);

			m_Rigidbody.position = rigidBody.Pose.Position;
			m_Rigidbody.rotation = rigidBody.Pose.Rotation;
			m_Rigidbody.velocity = rigidBody.Velocity;
			m_Rigidbody.angularVelocity = rigidBody.AngularVelocity;
		}

		private FakeRigidBody CreateFakeRigidBody()
		{
			return new FakeRigidBody
			{
				IsKinematic = m_Rigidbody.isKinematic,

				Pose = m_Rigidbody.ToPose(),
				PreviousPose = m_Rigidbody.ToPose(),

				Velocity = m_Rigidbody.velocity,
				AngularVelocity = m_Rigidbody.angularVelocity,

				Drag = m_Rigidbody.drag,
				AngularDrag = m_Rigidbody.angularDrag,

				InverseMass = 1.0f / m_Rigidbody.mass,
				InverseInertiaTensor = 1.0f / (float3)m_Rigidbody.inertiaTensor,
			};
		}

		private FakeRigidBody UpdateFakeRigidBody(FakeRigidBody fakeRigidBody)
		{
			return new FakeRigidBody
			{
				IsKinematic = m_Rigidbody.isKinematic,

				Pose = m_Rigidbody.ToPose(),
				PreviousPose = fakeRigidBody.PreviousPose,

				Velocity = m_Rigidbody.velocity,
				AngularVelocity = m_Rigidbody.angularVelocity,

				Drag = m_Rigidbody.drag,
				AngularDrag = m_Rigidbody.angularDrag,

				InverseMass = 1.0f / m_Rigidbody.mass,
				InverseInertiaTensor = 1.0f / (float3)m_Rigidbody.inertiaTensor,
			};
		}
	}
}
