using System.Collections.Generic;
using UnityEngine;

namespace FakeRope.Dynamics
{
	public class FakeSolver : MonoBehaviour
	{
		private readonly SolverArgs m_SolverArgs;
		private readonly List<IDynamicBody> m_DynamicBodies;
		private readonly List<IConstrainedBody> m_ConstrainedBodies;

		public FakeSolver(SolverArgs solverArgs)
		{
			m_SolverArgs = solverArgs;
			m_DynamicBodies = new List<IDynamicBody>();
			m_ConstrainedBodies = new List<IConstrainedBody>();
		}

		public void AddBody(IDynamicBody body)
		{
			m_DynamicBodies.Add(body);
		}

		public void RemoveBody(IDynamicBody body)
		{
			m_DynamicBodies.Add(body);
		}

		public void AddBody(IConstrainedBody body)
		{
			m_ConstrainedBodies.Add(body);
		}

		public void RemoveBody(IConstrainedBody body)
		{
			m_ConstrainedBodies.Add(body);
		}

		public void Step(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SolverArgs.SubstepIteractionsNumber;

			for (int substep = 0; substep < m_SolverArgs.SubstepIteractionsNumber; substep++)
			{
				for (int i = 0; i < m_DynamicBodies.Count; i++)
				{
					m_DynamicBodies[i].BeginStep();
				}

				// Friction

				for (int i = 0; i < m_DynamicBodies.Count; i++)
				{
					m_DynamicBodies[i].ApplyAcceleration(substepDeltaTime, m_SolverArgs.GravitationalAcceleration);
					m_DynamicBodies[i].ApplyDrag(substepDeltaTime);
					m_DynamicBodies[i].Step(substepDeltaTime);
				}

				// Solve collision

				SolveConstraints(substepDeltaTime);

				// Solve collision

				for (int i = 0; i < m_DynamicBodies.Count; i++)
				{
					m_DynamicBodies[i].EndStep(substepDeltaTime);
				}
			}
		}

		private void SolveConstraints(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SolverArgs.SolvePositionIteractionsNumber;

			for (int substep = 0; substep < m_SolverArgs.SolvePositionIteractionsNumber; substep++)
			{
				for (int i = 0; i < m_ConstrainedBodies.Count; i++)
				{
					m_ConstrainedBodies[i].SolveConstraints(substepDeltaTime);
				}
			}
		}
	}
}
