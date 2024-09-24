using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public struct FakeParticle
	{
		public float3 PreviousPosition;
		public float3 Position;
		public float3 Velocity;
		public float InverseMass;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakeParticle(float3 position, float mass)
		{
			PreviousPosition = position;
			Position = position;
			Velocity = float3.zero;
			InverseMass = 1.0f / mass;
		}
	}

	public static partial class SoftBodyComputations
	{
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
