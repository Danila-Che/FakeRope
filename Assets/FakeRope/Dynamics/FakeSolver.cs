using System.Collections.Generic;

namespace Fake.Dynamics
{
	public class FakeSolver
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
			if (m_DynamicBodies.Contains(body) is false)
			{
				m_DynamicBodies.Add(body);
			}
		}

		public void RemoveBody(IDynamicBody body)
		{
			m_DynamicBodies.Add(body);
		}

		public void AddBody(IConstrainedBody body)
		{
			if (m_ConstrainedBodies.Contains(body) is false)
			{
				m_ConstrainedBodies.Add(body);
			}
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

				SolveDynamics(substepDeltaTime);

				// Solve collision

				SolveConstraints(substepDeltaTime);

				// Solve collision

				for (int i = 0; i < m_DynamicBodies.Count; i++)
				{
					m_DynamicBodies[i].EndStep(substepDeltaTime);
				}
			}
		}

		private void SolveDynamics(float deltaTime)
		{
			for (int i = 0; i < m_DynamicBodies.Count; i++)
			{
				m_DynamicBodies[i].ApplyAcceleration(deltaTime, m_SolverArgs.GravitationalAcceleration);
				m_DynamicBodies[i].ApplyDrag(deltaTime);
				m_DynamicBodies[i].Step(deltaTime);
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
