using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public class FakeRopeContainer
	{
		private readonly IRopeArgs m_RopeArgs;

		private readonly List<Entity> m_Particles;

		public FakeRopeContainer(IRopeArgs ropeArgs)
		{
			m_RopeArgs = ropeArgs;

			m_Particles = new List<Entity>();
		}

		public List<Entity> Particles => m_Particles;

		public void Create(FakeSoftBodySubSolver subSolver, float3 sourcePosition, float3 targetPosition)
		{
			CreateParticles(subSolver, sourcePosition, targetPosition);
		}

		private void CreateParticles(FakeSoftBodySubSolver subSolver, float3 sourcePosition, float3 targetPosition)
		{
			var vector = sourcePosition - targetPosition;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = (int)math.ceil(magnitude / m_RopeArgs.SpanDistance);
			var mass = m_RopeArgs.Mass;
			var drag = m_RopeArgs.Drag;

			m_Particles.Capacity = particleCount;

			for (int i = 0; i < particleCount; i++)
			{
				CreateParticle(subSolver, new FakeParticle(targetPosition + m_RopeArgs.SpanDistance * i * normal, mass, drag));
			}

			CreateParticle(subSolver, new FakeParticle(sourcePosition, mass, drag));
		}

		private void CreateParticle(FakeSoftBodySubSolver subSolver, FakeParticle particle)
		{
			var entity = subSolver.RequireEntity(particle);

			m_Particles.Add(entity);
		}
	}
}
