using FakePhysics.CollisionDetection;
using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics;
using Unity.Entities;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.Solver)]
	public class TEMP_FakeSolverController : MonoBehaviour
	{
		[SerializeField] private SolverArgs m_SolverArgs;

		private readonly FakeSolver m_Solver = new();
		private readonly FakeRigidBodySolver m_FakeRigidBodySolver = new();

		public FakeRigidBodySolver FakeRigidBodySolver => m_FakeRigidBodySolver;

		private void Awake()
		{
			m_Solver.RegisterArgs(m_SolverArgs);
			m_Solver.CreateWorld();
			m_FakeRigidBodySolver.RegisterTo(m_Solver);
		}

		private void OnDestroy()
		{
			m_Solver.Dispose();
		}

		private void FixedUpdate()
		{
			m_Solver.Step(Time.fixedDeltaTime);
		}

		public Entity RequireEnity(Rigidbody rigidbody, FakeBoxCollider boxCollider)
		{
			return m_FakeRigidBodySolver.RequireEnity(rigidbody, boxCollider);
		}

		public TEMP_FakeRigidBody Get(Entity entity)
		{
			return m_Solver.EntityManager.GetComponentData<TEMP_FakeRigidBody>(entity);
		}

		public void Set(Entity entity, TEMP_FakeRigidBody rigidBody)
		{
			m_Solver.EntityManager.SetComponentData(entity, rigidBody);
		}
	}
}
