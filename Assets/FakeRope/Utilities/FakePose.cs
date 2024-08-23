using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace FakeRope.Utilities
{
	public readonly struct FakePose : IEquatable<FakePose>
	{
		public static readonly FakePose k_Identity = new(float3.zero, quaternion.identity);

		public readonly float3 Position;
		public readonly quaternion Rotation;

		public FakePose(float3 position, quaternion quaternion)
		{
			Position = position;
			Rotation = quaternion;

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(FakePose other)
		{
			return Position.Equals(other.Position) && Rotation.Equals(other.Rotation);
		}

		public override bool Equals(object o)
		{
			return o is FakePose converted && Equals(converted);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return (int)Computations.Hash(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
			return CommonUtilities.ToNumberFormat(
				"FakePose(({0}f, {1}f, {2}f, {3}f),  ({4}f, {5}f, {6}f))",
				Rotation.value.x,
				Rotation.value.y,
				Rotation.value.z,
				Rotation.value.w,
				Position.x,
				Position.y,
				Position.z);
		}
	}

	public static partial class Computations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakePose Transform(FakePose pose, FakePose otherPose)
		{
			var position = Transform(pose, otherPose.Position);
			var quaternion = math.mul(pose.Rotation, otherPose.Rotation);

			return new FakePose(position, quaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 Transform(FakePose pose, float3 vector)
		{
			return Rotate(pose, vector) + pose.Position;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 Rotate(FakePose pose, float3 vector)
		{
			return math.mul(pose.Rotation, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 InverseTransform(FakePose pose, float3 vector)
		{
			return InverseRotate(pose, vector - pose.Position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 InverseRotate(FakePose pose, float3 vector)
		{
			var inverse = math.inverse(pose.Rotation);

			return math.mul(inverse, vector);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakePose Translate(FakePose pose, float3 vector)
		{
			return new FakePose(pose.Position + vector, pose.Rotation);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakePose SetRotation(FakePose pose, quaternion rotation)
		{
			return new FakePose(pose.Position, rotation);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Hash(FakePose pose)
		{
			return math.hash(pose.Rotation) + 0xC5C5394Bu * math.hash(pose.Position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint4 HashWide(FakePose pose)
		{
			return math.hashwide(pose.Rotation) + 0xC5C5394Bu * math.hashwide(pose.Position).xyzz;
		}
	}
}
