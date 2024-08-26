using Fake.Dynamics;
using Fake.RigidBodyDynamics;
using Fake.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Fake.SoftBodyDynamics
{
	public class FakeRope : IDynamicBody, IConstrainedBody
	{
		private readonly FakeJoint m_FakeJoint;
		private readonly RopeArgs m_RopeArgs;

		private readonly List<FakeParticle> m_Particles;
		private readonly List<FakeDistanceConstraint> m_DistanceConstraints;

		private bool m_IsDisposed;

		public FakeRope(FakeJoint fakeJoint, RopeArgs ropeArgs)
		{
			m_FakeJoint = fakeJoint;
			m_RopeArgs = ropeArgs;

			m_Particles = new List<FakeParticle>();
			m_DistanceConstraints = new List<FakeDistanceConstraint>();

			m_IsDisposed = false;
		}

		public List<FakeParticle> Particles => m_Particles;

		public void Dispose()
		{
			m_Particles.Clear();
			m_DistanceConstraints.Clear();

			m_IsDisposed = true;
		}

		public void BeginStep()
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.PreviousPosition = particle.Position;

				m_Particles[i] = particle;
			}
		}

		public void Step(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Position += particle.Velocity * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void EndStep(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity = (particle.Position - particle.PreviousPosition) / deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void SolveConstraints(float deltaTime)
		{
			CheckDisposed();

			if (m_FakeJoint == null)
			{
				SolveInnerConstraints();
			}
			else
			{
				m_FakeJoint.RecalculateGlobalPoses();

				SolverOuterConstraint(
					particleIndex: 0,
					m_FakeJoint.TargetBody,
					m_FakeJoint.TargetGlobalPose.Position,
					deltaTime);

				SolveInnerConstraints();

				SolverOuterConstraint(
					m_Particles.Count - 1,
					m_FakeJoint.AnchorBody,
					m_FakeJoint.AnchorGlobalPose.Position,
					deltaTime);
			}
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity += acceleration * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void ApplyDrag(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Count; i++)
			{
				var particle = m_Particles[i];

				var drag = particle.Velocity * m_RopeArgs.Drag;
				particle.Velocity -= particle.InverseMass * deltaTime * drag;

				m_Particles[i] = particle;
			}
		}

		public void CreateFromJoint()
		{
			if (m_FakeJoint == null)
			{
				throw new ArgumentNullException(nameof(m_FakeJoint));
			}

			m_FakeJoint.RecalculateGlobalPoses();

			CreateParticles(m_FakeJoint.AnchorGlobalPose.Position, m_FakeJoint.TargetGlobalPose.Position);
			CreateConstraints();
		}

		public void Create(float3 sourcePosition, float3 targetPosition)
		{
			CreateParticles(sourcePosition, targetPosition);
			CreateConstraints();
		}

		private void CreateParticles(float3 sourcePosition, float3 targetPosition)
		{
			var vector = sourcePosition - targetPosition;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = 1 + (int)math.ceil(magnitude / m_RopeArgs.SpanDistance);
			var mass = m_RopeArgs.Mass;

			m_Particles.Capacity = particleCount;

			for (int i = 0; i < particleCount; i++)
			{
				m_Particles.Add(new FakeParticle(targetPosition + m_RopeArgs.SpanDistance * i * normal, mass));
			}

			m_Particles.Add(new FakeParticle(sourcePosition, mass));
		}

		private void CreateConstraints()
		{
			m_DistanceConstraints.Capacity = m_Particles.Count - 1;

			for (int i = 0; i < m_Particles.Count - 1; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

				m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));
			}
		}

		private void CheckDisposed()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		private void SolveInnerConstraints()
		{
			for (int i = 0; i < m_DistanceConstraints.Count; i++)
			{
				var constraint = m_DistanceConstraints[i];

				var particle0 = m_Particles[constraint.Index0];
				var particle1 = m_Particles[constraint.Index1];

				var direction = particle1.Position - particle0.Position;
				var length = math.length(direction);

				if (length > 0.0f)
				{
					var error = (length - constraint.Distance) / length;

					var w = particle0.InverseMass + particle1.InverseMass;

					var k0 = particle0.InverseMass / w;
					var k1 = particle1.InverseMass / w;

					particle0.Position += k0 * error * direction;
					particle1.Position -= k1 * error * direction;

					m_Particles[constraint.Index0] = particle0;
					m_Particles[constraint.Index1] = particle1;
				}
			}
		}

		private void SolverOuterConstraint(int particleIndex, FakeRigidBody body, float3 globalPosition, float deltaTime)
		{
			var particle = m_Particles[particleIndex];
			var correction = Computations.CalculateCorrection(globalPosition, particle.Position);

			if (math.any(correction))
			{
				DynamicsComputations.ApplyBodyPairCorrection(
					body,
					null,
					correction,
					0.0f,
					deltaTime,
					globalPosition,
					particle.Position);

				var bodyInverseMass = body.IsKinematic ? 0.0f : body.GetInverseMass(math.normalize(correction), globalPosition);

				var w = bodyInverseMass + particle.InverseMass;
				var k = particle.InverseMass / w;

				particle.Position -= k * correction;

				m_Particles[particleIndex] = particle;
			}
		}
	}
}
