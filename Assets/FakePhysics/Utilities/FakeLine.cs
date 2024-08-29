using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.Utilities
{
	public readonly struct FakeLine
	{
		public readonly float3 Start;
		public readonly float3 Finish;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FakeLine(float3 start, float3 finish)
		{
			Start = start;
			Finish = finish;
		}
	}

	public static partial class Computations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetDirection(FakeLine line)
		{
			return math.normalize(line.Finish - line.Start);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetVector(FakeLine line)
		{
			return line.Finish - line.Start;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetLength(FakeLine line)
		{
			return math.length(line.Finish - line.Start);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetLineIntersection(FakeLine line0, FakeLine line1, out float3 intersectionPoint)
		{
			float3 p13 = line0.Start - line1.Start;
			float3 p43 = line1.Finish - line1.Start;

			if (math.lengthsq(p43) < math.EPSILON)
			{
				intersectionPoint = float3.zero;
				return false;
			}

			float3 p21 = line0.Finish - line0.Start;

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
			float3 pa = line0.Start + mua * p21;

			intersectionPoint = pa;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 Rotate(FakeLine sourceLine, FakeLine targetLine, float stiffness, float restAngleInRadians = 0f)
		{
			var directionA = GetDirection(sourceLine);
			var directionB = GetDirection(targetLine);

			var dotProduct = math.dot(directionA, directionB);

			var angle = math.acos(math.clamp(dotProduct, -1f, 1f));
			angle -= restAngleInRadians;
			angle = stiffness * angle;

			if (math.isnan(angle) || math.abs(angle) < 0.001f)
			{
				return GetVector(sourceLine);
			}

			var normal = math.normalize(math.cross(directionA, directionB));
			var rotation = quaternion.AxisAngle(normal, angle);

			return GetLength(sourceLine) * math.mul(rotation, directionA);
		}
	}
}
