using FakePhysics.Dynamics;
using FakePhysics.RigidBodyDynamics;
using FakePhysics.Utilities;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public class FakeRope : IDisposable, IDynamicBody, IConstrainedBody
	{
		[BurstCompile]
		private struct DistanceConstraintSolver : IJob
		{
			public NativeList<FakeParticle> Particles;
			[ReadOnly]
			public NativeList<FakeDistanceConstraint> DistanceConstraints;

			public void Execute()
			{
				for (int i = 0; i < DistanceConstraints.Length; i++)
				{
					var constraint = DistanceConstraints[i];

					var particle0 = Particles[constraint.Index0];
					var particle1 = Particles[constraint.Index1];

					var correction = SoftBodyComputations.CalculateDistanceConstraintCorrection(particle0, particle1, constraint.Distance);

					var w = particle0.InverseMass + particle1.InverseMass;

					var k0 = particle0.InverseMass / w;
					var k1 = particle1.InverseMass / w;

					particle0.Position -= k0 * correction;
					particle1.Position += k1 * correction;

					Particles[constraint.Index0] = particle0;
					Particles[constraint.Index1] = particle1;
				}
			}
		}

		[BurstCompile]
		private struct BendConstraintSolver : IJob
		{
			public NativeList<FakeParticle> Particles;
			[ReadOnly]
			public NativeList<FakeBendConstraint> BendConstraints;
			public float Stiffness;

			public void Execute()
			{
				for (int i = 0; i < BendConstraints.Length; i++)
				{
					var constraint = BendConstraints[i];

					var particle0 = Particles[constraint.Index0];
					var particle1 = Particles[constraint.Index1];
					var particle2 = Particles[constraint.Index2];

					var correction = SoftBodyComputations.CalculateBendGradient(particle0.Position, particle1.Position, particle2.Position);

					var w = particle0.InverseMass + particle1.InverseMass + particle2.InverseMass;

					var k0 = particle0.InverseMass / w;
					var k1 = 2f * particle1.InverseMass / w;
					var k2 = particle2.InverseMass / w;

					particle0.Position += k0 * Stiffness * correction;
					particle1.Position -= k1 * Stiffness * correction;
					particle2.Position += k2 * Stiffness * correction;

					Particles[constraint.Index0] = particle0;
					Particles[constraint.Index1] = particle1;
					Particles[constraint.Index2] = particle2;
				}
			}
		}

		private readonly FakeJoint m_FakeJoint;
		private readonly RopeArgs m_RopeArgs;

		private NativeList<FakeParticle> m_Particles;
		private NativeList<FakeDistanceConstraint> m_DistanceConstraints;
		private NativeList<FakeBendConstraint> m_BendConstraints;

		private bool m_IsDisposed;
		private JobHandle m_Job;

		public FakeRope(FakeJoint fakeJoint, RopeArgs ropeArgs)
		{
			m_FakeJoint = fakeJoint;
			m_RopeArgs = ropeArgs;

			m_Particles = new NativeList<FakeParticle>(Allocator.Persistent);
			m_DistanceConstraints = new NativeList<FakeDistanceConstraint>(Allocator.Persistent);
			m_BendConstraints = new NativeList<FakeBendConstraint>(Allocator.Persistent);

			m_IsDisposed = false;
		}

		public NativeArray<FakeParticle>.ReadOnly Particles
		{
			get
			{
				m_Job.Complete();
				return m_Particles.AsParallelReader();
			}
		}

		public void Dispose()
		{
			m_Job.Complete();

			if (m_Particles.IsCreated)
			{
				m_Particles.Dispose();
			}

			if (m_DistanceConstraints.IsCreated)
			{
				m_DistanceConstraints.Dispose();
			}

			if (m_BendConstraints.IsCreated)
			{
				m_BendConstraints.Dispose();
			}

			m_IsDisposed = true;
		}

		public void BeginStep()
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Length; i++)
			{
				var particle = m_Particles[i];

				particle.PreviousPosition = particle.Position;

				m_Particles[i] = particle;
			}
		}

		public void Step(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Length; i++)
			{
				var particle = m_Particles[i];

				particle.Position += particle.Velocity * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void EndStep(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Length; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity = (particle.Position - particle.PreviousPosition) / deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void SolveConstraints(float deltaTime)
		{
			CheckDisposed();

			m_Job.Complete();

			if (m_FakeJoint == null)
			{
				m_Job = new BendConstraintSolver
				{
					Particles = m_Particles,
					BendConstraints = m_BendConstraints,
					Stiffness = m_RopeArgs.Stiffness,
				}.Schedule();

				m_Job = new DistanceConstraintSolver
				{
					Particles = m_Particles,
					DistanceConstraints = m_DistanceConstraints,
				}.Schedule(m_Job);
			}
			else
			{
				m_FakeJoint.RecalculateGlobalPoses();

				m_Job = new BendConstraintSolver
				{
					Particles = m_Particles,
					BendConstraints = m_BendConstraints,
					Stiffness = m_RopeArgs.Stiffness,
				}.Schedule();
				m_Job.Complete();

				SolveOuterConstraint(
					particleIndex: 0,
					m_FakeJoint.TargetBody,
					m_FakeJoint.TargetGlobalPose.Position,
					deltaTime);

				m_Job = new DistanceConstraintSolver
				{
					Particles = m_Particles,
					DistanceConstraints = m_DistanceConstraints,
				}.Schedule();
				m_Job.Complete();

				SolveOuterConstraint(
					m_Particles.Length - 1,
					m_FakeJoint.AnchorBody,
					m_FakeJoint.AnchorGlobalPose.Position,
					deltaTime);
			}
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Length; i++)
			{
				var particle = m_Particles[i];

				particle.Velocity += acceleration * deltaTime;

				m_Particles[i] = particle;
			}
		}

		public void ApplyDrag(float deltaTime)
		{
			CheckDisposed();

			for (int i = 0; i < m_Particles.Length; i++)
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

			if (distance <= 0f && m_DistanceConstraints.Length > 1)
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
			m_DistanceConstraints.Capacity = m_Particles.Length - 1;

			for (int i = 0; i < m_Particles.Length - 1; i++)
			{
				var distance = math.distance(m_Particles[i].Position, m_Particles[i + 1].Position);

				m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));
			}
		}

		private void CreateBendConstraints()
		{
			m_BendConstraints.Capacity = m_Particles.Length - 2;

			for (int i = 0; i < m_Particles.Length - 2; i++)
			{
				m_BendConstraints.Add(new FakeBendConstraint(i, i + 1, i + 2, 0f));
			}
		}

		private void AddParticle(float distance)
		{
			var i = m_Particles.Length - 1;
			var particle = m_Particles[i];

			m_Particles.Add(particle);
			m_DistanceConstraints.Add(new FakeDistanceConstraint(i, i + 1, distance));

			if (m_Particles.Length >= 3)
			{
				m_BendConstraints.Add(new FakeBendConstraint(i - 1, i, i + 1, 0f));
			}
		}

		private void RemoveParticle()
		{
			m_Particles.RemoveLastElement();
			m_DistanceConstraints.RemoveLastElement();
			m_BendConstraints.RemoveLastElement();
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
	}
}
