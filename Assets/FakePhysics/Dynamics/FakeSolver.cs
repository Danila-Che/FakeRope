using FakePhysics.CollisionDetection;
using FakePhysics.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace FakePhysics.Dynamics
{
	public class FakeSolver : IDisposable
	{
		public readonly struct Contact
		{
			public readonly ICollidingBody Body0;
			public readonly ICollidingBody Body1;
			public readonly float3 Point;
			public readonly float3 Normal;
			public readonly float Depth;
			public readonly float3 DeltaVDirection;
			public readonly float DeltaVLength;
			public readonly float Friction;

			public Contact(
				ICollidingBody body0,
				ICollidingBody body1,
				float3 point,
				float3 normal,
				float depth,
				float3 deltaVDirection,
				float deltaVLength,
				float friction)
			{
				Body0 = body0;
				Body1 = body1;
				Point = point;
				Normal = normal;
				Depth = depth;
				DeltaVDirection = deltaVDirection;
				DeltaVLength = deltaVLength;
				Friction = friction;
			}
		}

		private readonly List<IDynamicBody> m_DynamicBodies;
		private readonly List<ICollidingBody> m_CollidingBodies;
		private readonly List<IConstrainedBody> m_ConstrainedBodies;

		private readonly FakeCollisionDetectionSystem m_CollisionDetectionSystem;
		private readonly List<Contact> m_Contacts;

		private SolverArgs m_SolverArgs;

		private EntityManager m_EntityManager;
		private World m_CustomWorld;

		public FakeSolver()
		{
			m_DynamicBodies = new List<IDynamicBody>();
			m_CollidingBodies = new List<ICollidingBody>();
			m_ConstrainedBodies = new List<IConstrainedBody>();

			m_CollisionDetectionSystem = new FakeCollisionDetectionSystem();
			m_Contacts = new List<Contact>();
		}

		public List<Contact> ContactPairs => m_Contacts;

		public EntityManager EntityManager => m_EntityManager;

		public World DynamicWorld => m_CustomWorld;

		public void CreateWorld()
		{
			m_CustomWorld = new World("Dynamic World");
			m_EntityManager = m_CustomWorld.EntityManager;
		}

		public void Dispose()
		{
			if (m_CustomWorld.IsCreated)
			{
				m_CustomWorld.Dispose();
			}
		}

		public void RegisterArgs(SolverArgs solverArgs)
		{
			m_SolverArgs = solverArgs;
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
			m_DynamicBodies.Remove(body);
		}

		public void Remove(ICollidingBody body)
		{
			m_CollidingBodies.Remove(body);
		}

		public void Remove(IConstrainedBody body)
		{
			m_ConstrainedBodies.Remove(body);
		}

		public void Step(float deltaTime)
		{
			var substepDeltaTime = deltaTime / m_SolverArgs.SubstepIteractionsNumber;

			for (int substep = 0; substep < m_SolverArgs.SubstepIteractionsNumber; substep++)
			{
				BeginStep();
				Iterate(substepDeltaTime);
				m_Contacts.Clear();
				SolveCollisions(deltaTime);
				EndStep(substepDeltaTime);
			}
		}

		private void BeginStep()
		{
			Profiler.BeginSample("Begin Step");

			for (int i = 0; i < m_DynamicBodies.Count; i++)
			{
				m_DynamicBodies[i].BeginStep();
			}

			Profiler.EndSample();
		}

		private void Iterate(float deltaTime)
		{
			Profiler.BeginSample("Step");

			SolveFriction(deltaTime);
			SolveDynamics(deltaTime);
			SolveInnerConstraints(deltaTime);
			SolveOuterConstraints(deltaTime);

			Profiler.EndSample();
		}

		private void EndStep(float deltaTime)
		{
			Profiler.BeginSample("End Step");

			for (int i = 0; i < m_DynamicBodies.Count; i++)
			{
				m_DynamicBodies[i].EndStep(deltaTime);
			}

			Profiler.EndSample();
		}

		private void SolveFriction(float deltaTime)
		{
			Profiler.BeginSample("Friction");

			for (int i = 0; i < m_Contacts.Count; i++)
			{
				var contact = m_Contacts[i];
				var limit = contact.Friction * deltaTime;

				limit = contact.Body0?.CalculateFirctionForceLimit(
					limit,
					contact.Normal,
					contact.Point,
					contact.DeltaVDirection,
					contact.DeltaVLength) ?? limit;

				limit = contact.Body1?.CalculateFirctionForceLimit(
					limit,
					-contact.Normal,
					contact.Point,
					-contact.DeltaVDirection,
					contact.DeltaVLength) ?? limit;

				contact.Body0?.ApplyCorrection(limit * contact.DeltaVDirection, contact.Point, true);
				contact.Body1?.ApplyCorrection(-limit * contact.DeltaVDirection, contact.Point, true);
			}

			Profiler.EndSample();
		}

		private void SolveDynamics(float deltaTime)
		{
			Profiler.BeginSample("Dynamics");

			for (int i = 0; i < m_DynamicBodies.Count; i++)
			{
				m_DynamicBodies[i].ApplyAcceleration(deltaTime, m_SolverArgs.GravitationalAcceleration);
				m_DynamicBodies[i].ApplyDrag(deltaTime);
				m_DynamicBodies[i].Step(deltaTime);
			}

			Profiler.EndSample();
		}

		private void SolveInnerConstraints(float deltaTime)
		{
			Profiler.BeginSample("Inner Constraints");

			var substepDeltaTime = deltaTime / m_SolverArgs.SolvePositionIteractionsNumber;

			for (int substep = 0; substep < m_SolverArgs.SolvePositionIteractionsNumber; substep++)
			{
				for (int i = 0; i < m_ConstrainedBodies.Count; i++)
				{
					m_ConstrainedBodies[i].SolveInnerConstraints(substepDeltaTime);
				}
			}

			Profiler.EndSample();
		}

		private void SolveOuterConstraints(float deltaTime)
		{
			Profiler.BeginSample("Outer Constraints");

			var substepDeltaTime = deltaTime / m_SolverArgs.SolvePositionIteractionsNumber;

			for (int substep = 0; substep < m_SolverArgs.SolvePositionIteractionsNumber; substep++)
			{
				for (int i = 0; i < m_ConstrainedBodies.Count; i++)
				{
					m_ConstrainedBodies[i].SolveOuterConstraints(substepDeltaTime);
				}
			}

			Profiler.EndSample();
		}

		private void SolveCollisions(float deltaTime)
		{
			Profiler.BeginSample("Collisions");

			//var substepDeltaTime = deltaTime / m_SolverArgs.SolverCollisionIteractionNumber;
			var hasCollisions = true;

			for (int substep = 0; substep < m_SolverArgs.SolverCollisionIteractionNumber; substep++)
			{
				if (hasCollisions is false) { break; }

				hasCollisions = false;

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

						hasCollisions |= SolveCollisions(body0, body1, deltaTime);
					}
				}
			}

			Profiler.EndSample();
		}

		private bool SolveCollisions(ICollidingBody body0, ICollidingBody body1, float deltaTime)
		{
			if (m_CollisionDetectionSystem.TryGetCollisionPoints(
				body0.BoxCollider,
				body0.Pose,
				body1.BoxCollider,
				body1.Pose,
				out FakeContactPair contactPair))
			{
				if (contactPair.PenetrationDepth > math.EPSILON)
				{
					var origin = contactPair.Points.GetOrigin();

					DynamicsComputations.ApplyBodyPairCorrection(
						body0,
						body1,
						-contactPair.Normal * contactPair.PenetrationDepth,
						m_SolverArgs.Compliance,
						deltaTime,
						origin,
						origin);

					AddContact(body0, body1, origin, contactPair.Normal, contactPair.PenetrationDepth);

					return true;
				}
			}

			return false;
		}

		private void AddContact(ICollidingBody body0, ICollidingBody body1, float3 point, float3 normal, float depth)
		{
			var pointVelocity0 = body0?.GetVelocityAt(point) ?? float3.zero;
			var pointVelocity1 = body1?.GetVelocityAt(point) ?? float3.zero;
			var projection0 = math.dot(pointVelocity0, normal);
			var projection1 = math.dot(pointVelocity1, normal);

			var deltaV = pointVelocity0 - pointVelocity1 - (projection0 - projection1) * normal;
			var deltaVLength = math.length(deltaV);
			var deltaVDirection = float3.zero;

			if (deltaVLength != 0f)
			{
				deltaVDirection = deltaV / deltaVLength;
			}

			m_Contacts.Add(new Contact(
				body0.IsKinematic ? null : body0,
				body1.IsKinematic ? null : body1,
				point,
				normal,
				math.max(0f, depth),
				deltaVDirection,
				deltaVLength,
				-m_SolverArgs.Friction));
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

			if (correctionLength < math.EPSILON)
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

			if (w < math.EPSILON || math.isnan(w))
			{
				return;
			}

			var lambda = correctionLength / (w + compliance / (deltaTime * deltaTime));

			if (lambda < math.EPSILON || math.isnan(lambda))
			{
				return;
			}

			normal *= lambda;

			body0?.ApplyCorrection(normal, position0, velocityLevel);

			if (body1 != null)
			{
				normal = -normal;
				body1.ApplyCorrection(normal, position1, velocityLevel);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyBodyPairCorrection(
			ICollidingBody body0,
			float body1InverseMass,
			float3 correction,
			float compliance,
			float deltaTime,
			float3? position0 = null,
			bool velocityLevel = false)
		{
			var correctionLength = math.length(correction);

			if (correctionLength == 0f)
			{
				return;
			}

			body0 = body0 == null ? null : body0.IsKinematic ? null : body0;

			if (body0 == null) { return; }

			var normal = correction / correctionLength;

			var w0 = body0 == null ? 0f : body0.GetInverseMass(normal, position0);
			var w1 = body1InverseMass;

			var w = w0 + w1;

			if (w == 0f || math.isnan(w))
			{
				return;
			}

			var lambda = correctionLength / (w + compliance / (deltaTime * deltaTime));
			normal *= lambda;

			body0?.ApplyCorrection(normal, position0, velocityLevel);
		}
	}
}
