using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public class FakeRope : IDisposable, IDynamicBody, IConstrainedBody
	{
		private readonly FakeJoint m_FakeJoint;
		private readonly RopeArgs m_RopeArgs;

		private readonly List<FakeParticle> m_Particles;
		private readonly List<FakeDistanceConstraint> m_DistanceConstraints;
		private readonly List<FakeBendConstraint> m_BendConstraints;

		private bool m_IsDisposed;

		public FakeRope(FakeJoint fakeJoint, RopeArgs ropeArgs)
		{
			m_FakeJoint = fakeJoint;
			m_RopeArgs = ropeArgs;

			m_Particles = new List<FakeParticle>();
			m_DistanceConstraints = new List<FakeDistanceConstraint>();
			m_BendConstraints = new List<FakeBendConstraint>();

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
				SolveBendConstraint();
				SolveInnerConstraints();
			}
			else
			{
				m_FakeJoint.RecalculateGlobalPoses();

				SolveBendConstraint();

				SolveOuterConstraint(
					particleIndex: 0,
					m_FakeJoint.TargetBody,
					m_FakeJoint.TargetGlobalPose.Position,
					deltaTime);

				SolveInnerConstraints();

				SolveOuterConstraint(
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

		public void ChangeLength(float lengthDelta)
		{
			CheckDisposed();

			var constraint = m_DistanceConstraints[^1];
			var distance = constraint.Distance + lengthDelta;

			if (distance <= 0f && m_DistanceConstraints.Count > 1)
			{
				RemoveParticle();

				constraint = m_DistanceConstraints[^1];
				constraint.Distance = m_RopeArgs.SpanDistance - distance;
				m_DistanceConstraints[^1] = constraint;
			}
			else if (distance > m_RopeArgs.SpanDistance)
			{
				var remainder = distance - m_RopeArgs.SpanDistance;
				constraint.Distance = m_RopeArgs.SpanDistance;

				m_DistanceConstraints[^1] = constraint;

				AddParticle(remainder);
			}
			else
			{
				constraint.Distance = math.clamp(distance, 0f, m_RopeArgs.SpanDistance);

				m_DistanceConstraints[^1] = constraint;
			}
		}

		public void CreateFromJoint()
		{
			CheckDisposed();

			if (m_FakeJoint == null)
			{
				throw new ArgumentNullException(nameof(m_FakeJoint));
			}

			m_FakeJoint.RecalculateGlobalPoses();

			CreateParticles(m_FakeJoint.AnchorGlobalPose.Position, m_FakeJoint.TargetGlobalPose.Position);
			CreateDistanceConstraints();
			CreateBendConstraints();
		}

		public void Create(float3 sourcePosition, float3 targetPosition)
		{
			CheckDisposed();

			CreateParticles(sourcePosition, targetPosition);
			CreateDistanceConstraints();
			CreateBendConstraints();
		}

		private void CheckDisposed()
		{
			if (m_IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		private void CreateParticles(float3 sourcePosition, float3 targetPosition)
		{
			var vector = sourcePosition - targetPosition;
			var normal = math.normalize(vector);
			var magnitude = math.length(vector);
			var particleCount = (int)math.ceil(magnitude / m_RopeArgs.SpanDistance);
			var mass = m_RopeArgs.Mass;

			m_Particles.Capacity = particleCount;

			for (int i = 0; i < particleCount; i++)
			{
				m_Particles.Add(new FakeParticle(targetPosition + m_RopeArgs.SpanDistance * i * normal, mass));
			}

			m_Particles.Add(new FakeParticle(sourcePosition, mass));
		}

		private void CreateDistanceConstraints()
		{
			m_DistanceConstraints.Capacity = m_Particles.Count - 1;

			for (int i = 0; i < m_Particles.Count - 1; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

				m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));
			}
		}

		private void CreateBendConstraints()
		{
			m_BendConstraints.Capacity = m_Particles.Count - 2;

			for (int i = 0; i < m_Particles.Count - 2; i++)
			{
				m_BendConstraints.Add(new FakeBendConstraint(i, i + 1, i + 2, 0f));
			}
		}

		private void AddParticle(float distance)
		{
			var i = m_Particles.Count - 1;
			var particle = m_Particles[i];

			m_Particles.Add(particle);
			m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));
		}

		private void RemoveParticle()
		{
			m_DistanceConstraints.RemoveLastElement();
			m_Particles.RemoveLastElement();
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

				if (length > 0f)
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

		private void SolveOuterConstraint(int particleIndex, FakeRigidBody body, float3 globalPosition, float deltaTime)
		{
			var particle = m_Particles[particleIndex];
			var correction = Computations.CalculateCorrection(globalPosition, particle.Position);

			if (math.any(correction))
			{
				DynamicsComputations.ApplyBodyPairCorrection(
					body,
					null,
					correction,
					0f,
					deltaTime,
					globalPosition,
					particle.Position);

				var bodyInverseMass = body.IsKinematic ? 0f : body.GetInverseMass(math.normalize(correction), globalPosition);

				var w = bodyInverseMass + particle.InverseMass;
				var k = particle.InverseMass / w;

				particle.Position -= k * correction;

				m_Particles[particleIndex] = particle;
			}
		}

		private void SolveBendConstraint()
		{
			for (int i = 0; i < m_BendConstraints.Count; i++)
			{
				var constraint = m_BendConstraints[i];

				var particle0 = m_Particles[constraint.Index0];
				var particle1 = m_Particles[constraint.Index1];
				var particle2 = m_Particles[constraint.Index2];

				var line0 = new FakeLine(particle0.Position, particle1.Position);
				var line1 = new FakeLine(particle0.Position, particle2.Position);

				var correction = Computations.Rotate(line0, line1, m_RopeArgs.Stiffness);

				particle1.Position = particle0.Position + correction;

				m_Particles[constraint.Index1] = particle1;
			}
		}
	}
}
