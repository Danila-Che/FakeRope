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
		[SerializeField] private bool m_ShowDebug;

		private readonly FakeSolver m_Solver = new();

		private void Awake()
		{
			m_Solver.RegisterArgs(m_SolverArgs);
		}

		private void FixedUpdate()
		{
			BeforeStep?.Invoke();

			m_Solver.Step(Time.fixedDeltaTime);

			AfterStep?.Invoke();
		}

#if UNITY_EDITOR

		private void OnDrawGizmos()
		{
			if (m_Solver == null || m_ShowDebug is false)
			{
				return;
			}

			Gizmos.color = Color.green;

			foreach (var contactPair in m_Solver.ContactPairs)
			{
				foreach (var contact in contactPair.Points)
				{
					Gizmos.DrawRay(contact, contactPair.Normal);
				}
			}

			Gizmos.color = Color.red;

			foreach (var contactPair in m_Solver.ContactPairs)
			{
				foreach (var contact in contactPair.Points)
				{
					Gizmos.DrawRay(contact, contactPair.Normal * contactPair.PenetrationDepth);
				}
			}

			m_Solver.ContactPairs.Clear();
		}

#endif

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
