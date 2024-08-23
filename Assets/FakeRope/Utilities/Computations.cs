using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakeRope.Utilities
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
	}
}
