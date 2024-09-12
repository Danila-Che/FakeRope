using FakePhysics.CollisionDetection;
using FakePhysics.Utilities;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.RigidBodyDynamics
{
	public struct TEMP_FakeRigidBody : IComponentData
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

		public static Entity Create(EntityManager entityManager, Rigidbody rigidbody, FakeBoxCollider boxCollider)
		{
			var entity = entityManager.CreateEntity(typeof(TEMP_FakeRigidBody));
			entityManager.SetComponentData(entity, Create(rigidbody, boxCollider));

			return entity;
		}

		public static TEMP_FakeRigidBody Create(Rigidbody rigidbody, FakeBoxCollider boxCollider)
		{
			var inverseInertiaTensor = CollisionComputations.CalculateInverseInertiaTensor(boxCollider, rigidbody.mass);

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationX))
			{
				inverseInertiaTensor.x = 0f;
			}

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationY))
			{
				inverseInertiaTensor.y = 0f;
			}

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationZ))
			{
				inverseInertiaTensor.z = 0f;
			}

			return new TEMP_FakeRigidBody
			{
				IsKinematic = rigidbody.isKinematic,
				Pose = rigidbody.ToPose(),
				PreviousPose = rigidbody.ToPose(),

				Velocity = float3.zero,
				AngularVelocity = float3.zero,

				Drag = rigidbody.drag,
				AngularDrag = rigidbody.angularDrag,

				InverseMass = RigidBodyComputations.CalculateInverseMass(rigidbody.mass),
				InverseInertiaTensor = inverseInertiaTensor,
			};
		}

		public static TEMP_FakeRigidBody Create(FakePose pose, FakeBoxCollider boxCollider)
		{
			return new TEMP_FakeRigidBody
			{
				IsKinematic =  true,
				Pose = pose,
				PreviousPose = pose,

				Velocity = float3.zero,
				AngularVelocity = float3.zero,

				Drag = float3.zero,
				AngularDrag = float3.zero,

				InverseMass =  0f,
				InverseInertiaTensor = float3.zero,
			};
		}

		public static TEMP_FakeRigidBody Create(FakePose pose, FakeBoxCollider boxCollider, float mass)
		{
			return new TEMP_FakeRigidBody
			{
				IsKinematic = true,
				Pose = pose,
				PreviousPose = pose,

				Velocity = float3.zero,
				AngularVelocity = float3.zero,

				Drag = float3.zero,
				AngularDrag = float3.zero,

				InverseMass = RigidBodyComputations.CalculateInverseMass(mass),
				InverseInertiaTensor = CollisionComputations.CalculateInverseInertiaTensor(boxCollider, mass),
			};
		}
	}

	public static partial class RigidBodyComputations
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakePose ApplyRotation(FakePose pose, float3 rotation, float scale = 1f)
		{
			//scale = LimitRotationScale(rotation, scale);

			//var deltaQuaternion = quaternion.EulerXYZ(rotation * scale);
			//var result = math.mul(deltaQuaternion, pose.Rotation);
			//result = math.normalize(result);

			//return Computations.SetRotation(pose, result);

			scale = LimitRotationScale(rotation, scale);

			var deltaQuaternion = new quaternion(
				x: rotation.x * scale,
				y: rotation.y * scale,
				z: rotation.z * scale,
				w: 0f);

			deltaQuaternion = math.mul(deltaQuaternion, pose.Rotation);

			var result = new quaternion(
				x: pose.Rotation.value.x + 0.5f * deltaQuaternion.value.x,
				y: pose.Rotation.value.y + 0.5f * deltaQuaternion.value.y,
				z: pose.Rotation.value.z + 0.5f * deltaQuaternion.value.z,
				w: pose.Rotation.value.w + 0.5f * deltaQuaternion.value.w);
			result = math.normalize(result);

			return Computations.SetRotation(pose, result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FakePose ApplyAngularVelocity(FakePose pose, float3 rotation, float deltaTime = 1f)
		{
			var axis = rotation;
			var angle = math.length(rotation);

			if (angle < math.EPSILON)
			{
				return pose;
			}

			axis = math.normalize(axis);

			var ot = angle * deltaTime;
			var s = math.sin(ot * 0.5f);
			var c = math.cos(ot * 0.5f);
			var q = new float4(s * axis, c);
			var result = math.mul(pose.Rotation, q);

			return Computations.SetRotation(pose, result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LimitRotationScale(float3 rotation, float scale = 1f)
		{
			var phi = math.length(rotation);

			if (phi * scale > k_MaxRotationPerSubstep)
			{
				return k_MaxRotationPerSubstep / phi;
			}

			return scale;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CalculateInverseMass(float mass)
		{
			return math.isfinite(mass) switch
			{
				true => 1f / mass,
				false => 0f,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float CalculateInverseInertiaTensor(float mass)
		{
			return math.isfinite(mass) switch
			{
				true => 1f / mass,
				false => 0f,
			};
		}
	}
}
