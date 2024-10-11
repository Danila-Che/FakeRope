using System;
using System.Collections.Generic;
using Unity.Entities;

namespace FakePhysics.ECS.Dynamics
{
	public class FakeSolver : IDisposable
	{
		private readonly SolverArgs m_SolverArgs;

		private readonly World m_World;

		private readonly HashSet<IFakeSubSolver> m_SubSolvers;
		private readonly HashSet<IFakeDynamicSubSolver> m_FakeDynamicSubSolvers;
		private readonly HashSet<IFakeConstrainedSubSolver> m_FakeConstrainedSubSolvers;

		public FakeSolver(SolverArgs solverArgs)
		{
			m_SolverArgs = solverArgs;

			m_World = new World("Dynamic World");

			m_SubSolvers = new HashSet<IFakeSubSolver>();
			m_FakeDynamicSubSolvers = new HashSet<IFakeDynamicSubSolver>();
			m_FakeConstrainedSubSolvers = new HashSet<IFakeConstrainedSubSolver>();
		}

		public void Dispose()
		{
			m_SubSolvers.Clear();

			if (m_World.IsCreated)
			{
				m_World.Dispose();
			}
		}

		public void Register(IFakeSubSolver subSolver)
		{
			if (m_SubSolvers.Add(subSolver))
			{
				subSolver.Init(m_World);
			}
		}

		public void Register(IFakeDynamicSubSolver subSolver)
		{
			m_FakeDynamicSubSolvers.Add(subSolver);
		}

		public void Register(IFakeConstrainedSubSolver subSolver)
		{
			m_FakeConstrainedSubSolvers.Add(subSolver);
		}

		public T GetSubSolver<T>()
			where T : class, IFakeSubSolver
		{
			foreach (var subSolver in m_SubSolvers)
			{
				if (subSolver is T)
				{
					return subSolver as T;
				}
			}

			return null;
		}

		public void BeginStep()
		{
			foreach (var subSolver in m_FakeDynamicSubSolvers)
			{
				subSolver.BeginStep();
			}
		}

		public void Step(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SolverArgs.SubIterationsNumber;

			for (int substep = 0; substep < m_SolverArgs.SubIterationsNumber; substep++)
			{
				SolveDynamics(substepDeltaTime);
				SolveConstraints(substepDeltaTime);
			}
		}

		public void EndStep(float deltaTime)
		{
			foreach (var subSolver in m_FakeDynamicSubSolvers)
			{
				subSolver.EndStep(deltaTime);
			}
		}

		private void SolveDynamics(float deltaTime)
		{
			foreach (var subSolver in m_FakeDynamicSubSolvers)
			{
				subSolver.ApplyAcceleration(deltaTime, m_SolverArgs.GravitationalAcceleration);
				subSolver.ApplyDrag(deltaTime);
				subSolver.Step(deltaTime);
			}
		}

		private void SolveConstraints(float deltaTime)
		{
			foreach (var subSolver in m_FakeConstrainedSubSolvers)
			{
				subSolver.SolveConstraints(deltaTime);
			}
		}
	}
}
