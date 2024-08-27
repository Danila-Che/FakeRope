using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics;
using FakePhysics.SoftBodyDynamics;
using System;
using UnityEngine;

namespace FakePhysics.Controllers
{
	internal enum ExecutionOrder
	{
		Collider,
		Solver,
		RigidBody,
		SoftBody,
		Joint,
	}

	[DefaultExecutionOrder((int)ExecutionOrder.Solver)]
	internal class FakeSolverController : MonoBehaviour
	{
		public event Action BeforeStep;
		public event Action AfterStep;

		[SerializeField] private SolverArgs m_SolverArgs;

		private FakeSolver m_Solver;

		private void OnEnable()
		{
			m_Solver = new FakeSolver(m_SolverArgs);
		}

		private void FixedUpdate()
		{
			BeforeStep?.Invoke();

			m_Solver.Step(Time.fixedDeltaTime);

			AfterStep?.Invoke();
		}

		public void RegisterRigidBody(FakeRigidBody body)
		{
			m_Solver.Add((IDynamicBody)body);
			m_Solver.Add((ICollidingBody)body);
		}

		public void UnregisterRigidBody(FakeRigidBody body)
		{
			m_Solver.Remove((IDynamicBody)body);
			m_Solver.Remove((ICollidingBody)body);
		}

		public void RegisterSolfBody(FakeRope body)
		{
			m_Solver.Add((IDynamicBody)body);
			m_Solver.Add((IConstrainedBody)body);
		}

		public void UnregisterSolfBody(FakeRope body)
		{
			m_Solver.Remove((IDynamicBody)body);
			m_Solver.Remove((IConstrainedBody)body);
		}
	}
}
