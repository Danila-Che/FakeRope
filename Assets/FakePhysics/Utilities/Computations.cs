using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.Utilities
{
	public static partial class Computations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleInDegrees(float3 from, float3 to)
		{
			float num = math.sqrt(math.lengthsq(from) * math.lengthsq(to));

			if (num < 1E-15f)
			{
				return 0.0f;
			}

			float num2 = math.clamp(math.dot(from, to) / num, -1.0f, 1.0f);
			return math.degrees(math.acos(num2));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateCorrection(float3 p1, float3 p2, float distance)
		{
			var direction = p1 - p2;
			var length = math.length(direction);
			var error = (length - distance) / length;

			return error * direction;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CalculateCorrection(float3 p1, float3 p2)
		{
			return p1 - p2;
		}
	}
}
