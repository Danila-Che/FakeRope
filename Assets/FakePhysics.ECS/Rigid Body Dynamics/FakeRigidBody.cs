using FakePhysics.ECS.Utilities;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace FakePhysics.ECS.RigidBodyDynamics
{
	public struct FakeRigidBody : IComponentData
	{
		public bool IsKinematic;

		public FakePose Pose;
		public FakePose PreviousPose;

		public float3 Velocity;
		public float3 AngularVelocity;
		public float3 Drag;
		public float3 AngularDrag;

		public float InverseMass;
		public float3 InverseInertiaTensor;
	}

	public static partial class RigidBodyComputations
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ApplyBodyPairCorrection(
			ref FakeRigidBody rigidBody0,
			ref FakeRigidBody rigidBody1,
			float3 correction,
			float compliance,
			float deltaTime,
			float3 position0,
			float3 position1)
		{
			var correctionLength = math.length(correction);


			if (correctionLength < math.EPSILON)
			{
				return;
			}

			var normal = correction / correctionLength;

			var w0 = GetInverseMass(rigidBody0, normal, position0);
			var w1 = GetInverseMass(rigidBody1, normal, position0);

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

			rigidBody0 = ApplyCorrection(rigidBody0, normal, position0);

			normal = -normal;

			rigidBody1 = ApplyCorrection(rigidBody1, normal, position1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakeRigidBody ApplyCorrection(FakeRigidBody rigidBody, float3 correction, float3 position)
		{
			if (rigidBody.IsKinematic)
			{
				return rigidBody;
			}

			rigidBody.Pose = Computations.Translate(rigidBody.Pose, correction * rigidBody.InverseMass);

			var deltaQuaternion = math.cross(position - rigidBody.Pose.Position, correction);
			deltaQuaternion = Computations.InverseRotate(rigidBody.Pose, deltaQuaternion);
			deltaQuaternion *= rigidBody.InverseInertiaTensor;
			deltaQuaternion = Computations.Rotate(rigidBody.Pose, deltaQuaternion);

			return ApplyRotation(rigidBody, deltaQuaternion);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakeRigidBody ApplyRotation(FakeRigidBody rigidBody, float3 rotation, float scale = 1f)
		{
			if (rigidBody.IsKinematic)
			{
				return rigidBody;
			}

			var phi = math.length(rotation);

			if (phi * scale > k_MaxRotationPerSubstep)
			{
				scale = k_MaxRotationPerSubstep / phi;
			}

			var deltaQuaternion = new quaternion(
				x: rotation.x * scale,
				y: rotation.y * scale,
				z: rotation.z * scale,
				w: 0f);
			deltaQuaternion = math.mul(deltaQuaternion, rigidBody.Pose.Rotation);

			var quaternion = new quaternion(
				x: rigidBody.Pose.Rotation.value.x + 0.5f * deltaQuaternion.value.x,
				y: rigidBody.Pose.Rotation.value.y + 0.5f * deltaQuaternion.value.y,
				z: rigidBody.Pose.Rotation.value.z + 0.5f * deltaQuaternion.value.z,
				w: rigidBody.Pose.Rotation.value.w + 0.5f * deltaQuaternion.value.w);
			quaternion = math.normalize(quaternion);

			rigidBody.Pose = Computations.SetRotation(rigidBody.Pose, quaternion);

			return rigidBody;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetInverseMass(FakeRigidBody rigidBody, float3 normal, float3 position)
		{
			if (rigidBody.IsKinematic)
			{
				return 0.0f;
			}

			var nVector = math.cross(position - rigidBody.Pose.Position, normal);

			nVector = Computations.InverseRotate(rigidBody.Pose, nVector);

			var w =
				nVector.x * nVector.x * rigidBody.InverseInertiaTensor.x +
				nVector.y * nVector.y * rigidBody.InverseInertiaTensor.y +
				nVector.z * nVector.z * rigidBody.InverseInertiaTensor.z;

			w += rigidBody.InverseMass;

			return w;
		}
	}
}
