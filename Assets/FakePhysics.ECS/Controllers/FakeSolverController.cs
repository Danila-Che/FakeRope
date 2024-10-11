using FakePhysics.ECS.Dynamics;
using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.SoftBodyDynamics;
using System;
using UnityEngine;

namespace FakePhysics.ECS.Controllers
{
	[DefaultExecutionOrder((int)ExecutionOrder.Solver)]
	public class FakeSolverController : MonoBehaviour
	{
		public event Action StepStarting;
		public event Action Step;
		public event Action StepCompleted;

		[SerializeField] private SolverArgs m_SolverArgs;

		private FakeSolver m_FakeSolver;
		private FakeRigidBodySubSolver m_FakeRigidBodySubSolver;
		private FakeSoftBodySubSolver m_FakeSoftBodySubSolver;

		private void OnEnable()
		{
			m_FakeSolver = new FakeSolver(m_SolverArgs);
			m_FakeRigidBodySubSolver = new FakeRigidBodySubSolver();
			m_FakeSoftBodySubSolver = new FakeSoftBodySubSolver();

			m_FakeSolver.Register((IFakeSubSolver)m_FakeRigidBodySubSolver);
			m_FakeSolver.Register((IFakeDynamicSubSolver)m_FakeRigidBodySubSolver);
			m_FakeSolver.Register((IFakeConstrainedSubSolver)m_FakeRigidBodySubSolver);

			m_FakeSolver.Register((IFakeSubSolver)m_FakeSoftBodySubSolver);
			m_FakeSolver.Register((IFakeDynamicSubSolver)m_FakeSoftBodySubSolver);
			m_FakeSolver.Register((IFakeConstrainedSubSolver)m_FakeSoftBodySubSolver);
		}

		private void OnDisable()
		{
			m_FakeSolver.Dispose();
		}

		private void FixedUpdate()
		{
			StepStarting?.Invoke();

			m_FakeSolver.BeginStep();

			Step?.Invoke();

			m_FakeSolver.Step(Time.fixedDeltaTime);
			m_FakeSolver.EndStep(Time.fixedDeltaTime);

			StepCompleted?.Invoke();
		}

		public T GetSubSolver<T>()
			where T : class, IFakeSubSolver
		{
			return m_FakeSolver.GetSubSolver<T>();
		}
	}
}
