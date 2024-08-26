using Fake.CollisionDetection;
using Fake.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Fake.Dynamics
{
	public class FakeSolver
	{
		private readonly SolverArgs m_SolverArgs;

		private readonly List<IDynamicBody> m_DynamicBodies;
		private readonly List<ICollidingBody> m_CollidingBodies;
		private readonly List<IConstrainedBody> m_ConstrainedBodies;

		private readonly FakeCollisionDetectionSystem m_CollisionDetectionSystem;

		public FakeSolver(SolverArgs solverArgs)
		{
			m_SolverArgs = solverArgs;

			m_DynamicBodies = new List<IDynamicBody>();
			m_CollidingBodies = new List<ICollidingBody>();
			m_ConstrainedBodies = new List<IConstrainedBody>();

			m_CollisionDetectionSystem = new FakeCollisionDetectionSystem();
		}

		public void Add(IDynamicBody body)
		{
			if (m_DynamicBodies.Contains(body) is false)
			{
				m_DynamicBodies.Add(body);
			}
		}

		public void Add(ICollidingBody body)
		{
			if (m_CollidingBodies.Contains(body) is false)
			{
				m_CollidingBodies.Add(body);
			}
		}

		public void Add(IConstrainedBody body)
		{
			if (m_ConstrainedBodies.Contains(body) is false)
			{
				m_ConstrainedBodies.Add(body);
			}
		}

		public void Remove(IDynamicBody body)
		{
			m_DynamicBodies.Add(body);
		}

		public void Remove(ICollidingBody body)
		{
			m_CollidingBodies.Remove(body);
		}

		public void Remove(IConstrainedBody body)
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

				SolveDynamics(substepDeltaTime);
				SolveCollisions(substepDeltaTime);
				SolveConstraints(substepDeltaTime);

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

		private void SolveCollisions(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SolverArgs.SolverCollisionIteractionNumber;

			for (int substep = 0; substep < m_SolverArgs.SolverCollisionIteractionNumber; substep++)
			{
				for (int i = 0; i < m_CollidingBodies.Count - 1; i++)
				{
					var body0 = m_CollidingBodies[i];

					for (int j = i + 1; j < m_CollidingBodies.Count; j++)
					{
						var body1 = m_CollidingBodies[j];

						if (body0.IsKinematic && body1.IsKinematic)
						{
							continue;
						}

						if (m_CollisionDetectionSystem.TryGetCollisionPoints(
							body0.BoxCollider,
							body0.Pose,
							body1.BoxCollider,
							body1.Pose,
							out FakeContactPair contactPair))
						{
							var origin = contactPair.Points.GetOrigin();

							if (contactPair.PenetrationDepth > 0f)
							{
								DynamicsComputations.ApplyBodyPairCorrection(
									body0,
									body1,
									-contactPair.Normal * contactPair.PenetrationDepth,
									0f,
									substepDeltaTime,
									origin,
									origin);
							}
						}
					}
				}
			}
		}
	}

	public static partial class DynamicsComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyBodyPairCorrection(
			ICollidingBody body0,
			ICollidingBody body1,
			float3 correction,
			float compliance,
			float deltaTime,
			float3? position0 = null,
			float3? position1 = null,
			bool velocityLevel = false)
		{
			var correctionLength = math.length(correction);

			if (correctionLength == 0f)
			{
				return;
			}

			body0 = body0 == null ? null : body0.IsKinematic ? null : body0;
			body1 = body1 == null ? null : body1.IsKinematic ? null : body1;

			if (body0 == null && body1 == null)
			{
				return;
			}

			var normal = correction / correctionLength;

			var w0 = body0 == null ? 0f : body0.GetInverseMass(normal, position0);
			var w1 = body1 == null ? 0f : body1.GetInverseMass(normal, position1);

			var w = w0 + w1;

			if (w == 0f || math.isnan(w))
			{
				return;
			}

			var lambda = -correctionLength / (w + compliance / (deltaTime * deltaTime));
			normal *= -lambda;

			body0?.ApplyCorrection(normal, position0, velocityLevel);

			if (body1 != null)
			{
				normal = -normal;
				body1.ApplyCorrection(normal, position1, velocityLevel);
			}
	}
	}
}
