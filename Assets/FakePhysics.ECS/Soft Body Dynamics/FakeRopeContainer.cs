using System.Collections.Generic;
using System.Diagnostics;
using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.Utilities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public class FakeRopeContainer
	{
		private readonly IRopeArgs m_RopeArgs;

		private readonly List<FakeEntity> m_Particles;
		private readonly List<FakeEntity> m_DistanceConstraints;
		private readonly List<FakeEntity> m_BendConstraints;

		private FakeEntity m_SourceJoint;
		private FakeEntity m_TargetJoint;

		public FakeRopeContainer(IRopeArgs ropeArgs)
		{
			m_RopeArgs = ropeArgs;

			m_Particles = new List<FakeEntity>();
			m_DistanceConstraints = new List<FakeEntity>();
			m_BendConstraints = new List<FakeEntity>();
		}

		public List<FakeEntity> Particles => m_Particles;

		public void Create(FakeSoftBodySubSolver subSolver, FakeJoint joint, float3 sourcePosition, float3 targetPosition)
		{
			CreateParticles(subSolver, sourcePosition, targetPosition);
			CreateJoints(subSolver, joint);
			CreateDistanceConstraints(subSolver);
			CreateBendConstraints(subSolver);
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

		private void CreateJoints(FakeSoftBodySubSolver subSolver, FakeJoint joint)
		{
			m_TargetJoint = subSolver.RequireEntity(new FakeParticleJoint
			{
				Particle = m_Particles[0],
				AnchorBody = joint.TargetBody,
				AnchorLocalPose = joint.TargetLocalPose,
			});

			m_SourceJoint = subSolver.RequireEntity(new FakeParticleJoint
			{
				Particle = m_Particles[^1],
				AnchorBody = joint.AnchorBody,
				AnchorLocalPose = joint.AnchorLocalPose,
			});
		}

		private void CreateParticle(FakeSoftBodySubSolver subSolver, FakeParticle particle)
		{
			var entity = subSolver.RequireEntity(particle);

			m_Particles.Add(entity);
		}

		private void CreateDistanceConstraints(FakeSoftBodySubSolver subSolver)
		{
			m_DistanceConstraints.Capacity = m_Particles.Count - 1;

			for (int i = 0; i < m_Particles.Count - 1; i++)
			{
				var particle0 = subSolver.GetParticle(m_Particles[i]);
				var particle1 = subSolver.GetParticle(m_Particles[i + 1]);
				var distance = math.distance(particle0.Position, particle1.Position);

				var entity = subSolver.RequireEntity(new FakeDistanceConstraint
				{
					Particle0 = m_Particles[i],
					Particle1 = m_Particles[i + 1],
					Distance = distance,
				});

				m_DistanceConstraints.Add(entity);
			}
		}

		private void CreateBendConstraints(FakeSoftBodySubSolver subSolver)
		{
			m_BendConstraints.Capacity = m_Particles.Count - 2;

			for (int i = 0; i < m_Particles.Count - 2; i++)
			{
				var particle0 = subSolver.GetParticle(m_Particles[i]);
				var particle1 = subSolver.GetParticle(m_Particles[i + 1]);
				var particle2 = subSolver.GetParticle(m_Particles[i + 2]);

				var entity = subSolver.RequireEntity(new FakeBendConstraint
				{
					Particle0 = m_Particles[i],
					Particle1 = m_Particles[i + 1],
					Particle2 = m_Particles[i + 2],
					Stiffness = m_RopeArgs.Stiffness,
				});

				m_BendConstraints.Add(entity);
			}
		}
	}
}
