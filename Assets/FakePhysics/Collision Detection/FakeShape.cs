using FakePhysics.Utilities;
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakePhysics.CollisionDetection
{
	public struct FakeShape
	{
		public readonly FakeContact[] Points;
		private int m_Index;

		public FakeShape(int pointsCount)
		{
			Points = new FakeContact[pointsCount];
			m_Index = 0;
		}

		public readonly int Count => Points.Length;

		public readonly FakeContact this[int index]
		{
			get => Points[index];
			set => Points[index] = value;
		}

		public void Add(FakeContact contact)
		{
			Points[m_Index] = contact;
			m_Index++;
		}

		public void Add(float3 point)
		{
			Points[m_Index] = new FakeContact(point);
			m_Index++;
		}

		public readonly void Sort(Comparison<FakeContact> comparison)
		{
			Array.Sort(Points, comparison);
		}
	}

	public static partial class CollisionComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CalculateAngles(FakeShape shape, float3 normal)
		{
			var origin = FindOrigin(shape);
			var refVector = shape[0].Point - origin;

			for (int i = 1; i < shape.Count; i++)
			{
				var originToPoint = shape[i].Point - origin;
				var u = math.dot(normal, math.cross(refVector, originToPoint));
				var angle = Computations.AngleInDegrees(refVector, originToPoint);

				var contact = shape[i];

				if (u <= 0.001f)
				{
					contact.Angle = angle;
				}
				else
				{
					contact.Angle = angle + 180.0f;
				}

				shape[i] = contact;
			}
		}

		public static float3 FindOrigin(FakeShape shape)
		{
			var origin = float3.zero;

			for (int i = 0; i < shape.Count; i++)
			{
				origin += shape[i].Point;
			}

			origin /= shape.Count;
			return origin;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInside(FakeShape shape, float3 point, float3 normal, bool checkOnShape = false)
		{
			for (int i = 0; i < shape.Count; i++)
			{
				var j = i + 1;
				if (j == shape.Count)
				{
					j = 0;
				}

				var vector = point - shape[i].Point;
				var edgeVector = shape[j].Point - shape[i].Point;
				var direction = math.cross(math.normalize(edgeVector), normal);

				if (AreFacingDifferentDirections(direction, vector, checkOnShape))
				{
					return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreFacingDifferentDirections(float3 vector0, float3 vector1, bool checkPerpendicularity = false)
		{
			var dotProduct = math.dot(vector0, vector1);
			return checkPerpendicularity ? dotProduct < 0f : dotProduct <= 0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreFacingSameDirection(float3 vector0, float3 vector1, bool checkPerpendicularity = false)
		{
			var dotProduct = math.dot(vector0, vector1);
			return checkPerpendicularity ? dotProduct >= 0f : dotProduct > 0f;
		}
	}
}
