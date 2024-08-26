using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.Utilities
{
	public readonly struct AABB
	{
		public readonly float3 Min;
		public readonly float3 Max;

		public AABB(float3 min, float3 max)
		{
			Min = min;
			Max = max;
		}
	}

	public static partial class Computations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Intersects(AABB aabb0, AABB aabb1)
		{
			return
				aabb0.Min.x <= aabb1.Max.x &&
				aabb0.Max.x >= aabb1.Min.x &&
				aabb0.Min.y <= aabb1.Max.y &&
				aabb0.Max.y >= aabb1.Min.y &&
				aabb0.Min.z <= aabb1.Max.z &&
				aabb0.Max.z >= aabb1.Min.z;
		}
	}
}
