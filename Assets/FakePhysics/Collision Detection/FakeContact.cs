using FakePhysics.Utilities;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.CollisionDetection
{
	public struct FakeContact
	{
		public float3 Point;
		public float Angle;

		public FakeContact(float3 point)
		{
			Point = point;
			Angle = 0.0f;
		}

		public FakeContact(float3 point, float angle)
		{
			Point = point;
			Angle = angle;
		}

		public override readonly string ToString()
		{
			return CommonUtilities.ToNumberFormat(
				"FakeContact(({0}f, {1}f, {2}f), {3}f degrees)",
				Point.x,
				Point.y,
				Point.z,
				Angle);
		}
	}

	public static partial class CollisionComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompareAngle(FakeContact contact0, FakeContact contact1)
		{
			if (contact0.Angle < contact1.Angle)
			{
				return -1;
			}

			return 1;
		}
	}
}
