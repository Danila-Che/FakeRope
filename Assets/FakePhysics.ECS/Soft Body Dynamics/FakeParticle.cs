using System.Runtime.CompilerServices;
using FakePhysics.ECS.RigidBodyDynamics;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public struct FakeParticle
	{
		public float3 PreviousPosition;
		public float3 Position;
		public float3 Velocity;
		public float Drag;
		public float InverseMass;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakeParticle(float3 position, float mass, float drag)
		{
			PreviousPosition = position;
			Position = position;
			Velocity = float3.zero;
			Drag = drag;
			InverseMass = 1.0f / mass;
		}
	}

	public static partial class SoftBodyComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyBodyPairCorrection(
			ref FakeRigidBody rigidBody,
			ref FakeParticle particle,
			float3 correction,
			float compliance,
			float deltaTime,
			float3 position)
		{
			var correctionLength = math.length(correction);

			if (correctionLength < math.EPSILON)
			{
				return;
			}

			var normal = correction / correctionLength;

			var w0 = RigidBodyComputations.GetInverseMass(rigidBody, normal, position);
			var w1 = particle.InverseMass;

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

			rigidBody = RigidBodyComputations.ApplyCorrection(rigidBody, normal, position);

			normal = -normal;

			particle = ApplyCorrection(particle, normal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakeParticle ApplyCorrection(FakeParticle particle, float3 correction)
		{
			particle.Position += particle.InverseMass * correction;

			return particle;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateDistanceConstraintCorrection(FakeParticle particle0, FakeParticle particle1, float distance)
		{
			var error = CalculateDistanceConstraint(particle0.Position, particle1.Position, distance);
			var gradient = CalculateDistanceGradient(particle0.Position, particle1.Position);

			return error * gradient;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CalculateDistanceConstraint(float3 p1, float3 p2, float distance)
		{
			return math.length(p1 - p2) - distance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateDistanceGradient(float3 p1, float3 p2)
		{
			return math.normalize(p1 - p2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CalculateBendConstraint(float3 p1, float3 p2, float3 p3, float restAngleInRadians)
		{
			var n1 = math.normalize(p1 - p2);
			var n2 = math.normalize(p3 - p2);

			return math.acos(math.dot(n1, n2)) - restAngleInRadians;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateBendConstraintCorrection(float3 p1, float3 p2, float3 p3)
		{
			var centroid = (p1 + p2 + p3) / 3f;

			return p2 - centroid;
		}
	}
}
