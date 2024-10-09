using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics;
using FakePhysics.SoftBodyDynamics;
using System;
using UnityEngine;

namespace FakePhysics.Controllers
{
	public enum ExecutionOrder
	{
		Collider,
		Solver,
		RigidBody,
		SoftBody,
		Joint,
	}

	[DefaultExecutionOrder((int)ExecutionOrder.Solver)]
	public class FakeSolverController : MonoBehaviour
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

			foreach (var contactPair in m_Solver.ContactPairs)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawRay(contactPair.Point, contactPair.Normal);

				Gizmos.color = Color.red;
				Gizmos.DrawRay(contactPair.Point, contactPair.Normal * contactPair.Depth);
			}
		}

#endif

		public void RegisterRigidBody(FakeRigidBody body)
		{
			m_Solver.Add((IDynamicBody)body);
			m_Solver.Add((ICollidingBody)body);
		}

		public void RegisterSolfBody(FakeRope body)
		{
			m_Solver.Add((IDynamicBody)body);
			m_Solver.Add((IConstrainedBody)body);
		}

		public void RegisterSolfBody(FakeAttachmentConstraint constraint)
		{
			m_Solver.Add(constraint);
		}

		public void UnregisterRigidBody(FakeRigidBody body)
		{
			m_Solver.Remove((IDynamicBody)body);
			m_Solver.Remove((ICollidingBody)body);
		}

		public void UnregisterSolfBody(FakeRope body)
		{
			m_Solver.Remove((IDynamicBody)body);
			m_Solver.Remove((IConstrainedBody)body);
		}

		public void UnregisterSolfBody(FakeAttachmentConstraint constraint)
		{
			m_Solver.Remove(constraint);
		}
	}
}
