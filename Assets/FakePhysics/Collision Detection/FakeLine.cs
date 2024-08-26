using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.CollisionDetection
{
	public readonly struct FakeLine
	{
		public readonly float3 Point0;
		public readonly float3 Point1;

		public FakeLine(float3 point0, float3 point1)
		{
			Point0 = point0;
			Point1 = point1;
		}
	}

	public static partial class CollisionComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetVector(FakeLine line)
		{
			return math.normalize(line.Point1 - line.Point0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLineIntersection(FakeLine line0, FakeLine line1, out float3 intersectionPoint)
		{
			float3 p13 = line0.Point0 - line1.Point0;
			float3 p43 = line1.Point1 - line1.Point0;

			if (math.lengthsq(p43) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float3 p21 = line0.Point1 - line0.Point0;

			if (math.lengthsq(p21) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float d1343 = math.dot(p13, p43);
			float d4321 = math.dot(p43, p21);
			float d1321 = math.dot(p13, p21);
			float d4343 = math.dot(p43, p43);
			float d2121 = math.dot(p21, p21);

			float denom = d2121 * d4343 - d4321 * d4321;

			if (math.abs(denom) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float numer = d1343 * d4321 - d1321 * d4343;
			float mua = numer / denom;
			float3 pa = line0.Point0 + mua * p21;

			intersectionPoint = pa;
			return true;
		}
	}
}
