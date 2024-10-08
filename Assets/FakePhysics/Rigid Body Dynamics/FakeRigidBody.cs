using FakePhysics.CollisionDetection;
using FakePhysics.Dynamics;
using FakePhysics.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.RigidBodyDynamics
{
	public class FakeRigidBody : IDynamicBody, ICollidingBody
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		private bool m_IsKinematic;
		private FakeBoxCollider m_BoxCollider;

		private FakePose m_Pose;
		private FakePose m_PreviousPose;

		private float3 m_Velocity;
		private float3 m_AngularVelocity;
		private readonly float3 m_Drag;
		private readonly float3 m_AngularDrag;

		private readonly float m_InverseMass;
		private readonly float3 m_InverseInertiaTensor;

		public FakeRigidBody(Rigidbody rigidbody, FakeBoxCollider boxCollider)
			: this(rigidbody.ToPose(), boxCollider)
		{
			m_IsKinematic = rigidbody.isKinematic;
			m_Drag = rigidbody.drag;
			m_AngularDrag = rigidbody.angularDrag;

			m_BoxCollider = boxCollider;

			m_InverseMass = math.isfinite(rigidbody.mass) switch
			{
				true => 1f / rigidbody.mass,
				false => 0f,
			};

			m_InverseInertiaTensor = CollisionComputations.CalculateInverseInertiaTensor(boxCollider, rigidbody.mass);

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationX))
			{
				m_InverseInertiaTensor.x = 0f;
			}

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationY))
			{
				m_InverseInertiaTensor.y = 0f;
			}

			if (rigidbody.IsConstrainedBy(RigidbodyConstraints.FreezeRotationZ))
			{
				m_InverseInertiaTensor.z = 0f;
			}
		}

		public FakeRigidBody(FakePose pose, FakeBoxCollider boxCollider)
		{
			m_Pose = pose;
			m_PreviousPose = m_Pose;

			m_IsKinematic = true;
			m_Drag = 0f;
			m_AngularDrag = 0f;

			m_Velocity = float3.zero;
			m_AngularVelocity = float3.zero;

			m_BoxCollider = boxCollider;

			m_InverseMass = 0f;
			m_InverseInertiaTensor = CollisionComputations.CalculateInverseInertiaTensor(boxCollider, math.INFINITY);
		}

		public bool IsKinematic => m_IsKinematic;

		public bool IsTrigger { get => m_BoxCollider.IsTrigger; set => m_BoxCollider.IsTrigger = value; }

		public FakePose Pose => m_Pose;

		public float3 Velocity =>  m_Velocity;

		public float3 AngularVelocity => m_AngularVelocity;

		public FakeBoxCollider BoxCollider => m_BoxCollider;

		public float InverseMass => m_InverseMass;

		public float3 InverseInertiaTensor => m_InverseInertiaTensor;

		public void UpdateWith(Rigidbody rigidbody)
		{
			m_IsKinematic = rigidbody.isKinematic;
			m_Pose = rigidbody.ToPose();
			m_Velocity = rigidbody.velocity;
			m_AngularVelocity = rigidbody.velocity;
		}
		
		public void UpdateWith(FakePose pose)
		{
			m_Pose = pose;
		}

		public void BeginStep()
		{
			if (m_IsKinematic) { return; }

			m_PreviousPose = m_Pose;
		}

		public void Step(float deltaTime)
		{
			if (m_IsKinematic) { return; }

			m_Pose = Computations.Translate(m_Pose, m_Velocity * deltaTime);

			ApplyRotation(m_AngularVelocity, deltaTime);
		}

		public void EndStep(float deltaTime)
		{
			if (m_IsKinematic) { return; }

			m_Velocity = (m_Pose.Position - m_PreviousPose.Position) / deltaTime;

			var deltaQuaternion = math.mul(m_Pose.Rotation, math.inverse(m_PreviousPose.Rotation));
			m_AngularVelocity = new float3(
				x: 2f * deltaQuaternion.value.x / deltaTime,
				y: 2f * deltaQuaternion.value.y / deltaTime,
				z: 2f * deltaQuaternion.value.z / deltaTime);

			if (deltaQuaternion.value.w < 0f)
			{
				m_AngularVelocity = -m_AngularVelocity;
			}
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			m_Velocity += acceleration * deltaTime;
		}

		public void ApplyDrag(float deltaTime)
		{
			var drag = m_Velocity * m_Drag;
			m_Velocity -= m_InverseMass * deltaTime * drag;

			var angularDrag = m_AngularVelocity * m_AngularDrag;
			m_AngularVelocity -= m_InverseMass * deltaTime * angularDrag;
		}

		public float GetInverseMass(float3 normal, float3? position = null)
		{
			var nVector = position == null
				? normal
				: math.cross(position.Value - m_Pose.Position, normal);

			nVector = Computations.InverseRotate(m_Pose, nVector);

			var w =
				nVector.x * nVector.x * m_InverseInertiaTensor.x +
				nVector.y * nVector.y * m_InverseInertiaTensor.y +
				nVector.z * nVector.z * m_InverseInertiaTensor.z;

			if (position != null)
			{
				w += m_InverseMass;
			}

			return w;
		}

		public float CalculateFirctionForceLimit(
			float frictionMagnitude,
			float3 contactNormal,
			float3 contactPoint,
			float3 deltaVDirection,
			float deltaVMagnitude)
		{
			var beforePointV = GetVelocityAt(contactPoint);
			var beforeVelocity = m_Velocity;
			var beforeAngularVelocity = m_AngularVelocity;
			var correctionAmount = deltaVDirection * frictionMagnitude;

			ApplyCorrection(correctionAmount, contactPoint, true);

			var afterPointV = GetVelocityAt(contactPoint);
			var actualDeltaV = afterPointV - beforePointV;
			var actualTangDeltaV = actualDeltaV - contactNormal * math.dot(actualDeltaV, contactNormal);
			var actualTDVLenght = math.length(actualTangDeltaV);

			m_Velocity = beforeVelocity;
			m_AngularVelocity = beforeAngularVelocity;

			var reduction = actualTDVLenght == 0f ? 0f : math.clamp(deltaVMagnitude / actualTDVLenght, 0, 1);
			return reduction * frictionMagnitude;
		}

		public float3 GetVelocityAt(float3 position)
		{
			var velocity = math.cross(position - m_Pose.Position, m_AngularVelocity);

			return m_Velocity - velocity;
		}

		public void ApplyCorrection(float3 correction, float3? position = null, bool velocityLevel = false)
		{
			if (m_IsKinematic) { return; }

			ApplyCorrection(correction, position, m_InverseMass, velocityLevel);
		}

		public void ApplyCorrectionVelocity(float3 correction, float3? position = null, bool velocityLevel = false)
		{
			if (m_IsKinematic) { return; }

			ApplyCorrection(correction, position, inverseMass: 1f, velocityLevel);
		}

		private void ApplyCorrection(float3 correction, float3? position = null, float inverseMass = 1f, bool velocityLevel = false)
		{
			float3 deltaQuaternion;
			if (position == null)
			{
				deltaQuaternion = correction;
			}
			else
			{
				if (velocityLevel)
				{
					m_Velocity += correction * inverseMass;
				}
				else
				{
					m_Pose = Computations.Translate(m_Pose, correction * inverseMass);
				}

				deltaQuaternion = math.cross(position.Value - m_Pose.Position, correction);
			}

			deltaQuaternion = Computations.InverseRotate(m_Pose, deltaQuaternion);
			deltaQuaternion *= m_InverseInertiaTensor;
			deltaQuaternion = Computations.Rotate(m_Pose, deltaQuaternion);

			if (velocityLevel)
			{
				m_AngularVelocity += deltaQuaternion;
			}
			else
			{
				ApplyRotation(deltaQuaternion);
			}
		}

		private void ApplyRotation(float3 rotation, float scale = 1f)
		{
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
			deltaQuaternion = math.mul(deltaQuaternion, m_Pose.Rotation);

			var quaternion = new quaternion(
				x: m_Pose.Rotation.value.x + 0.5f * deltaQuaternion.value.x,
				y: m_Pose.Rotation.value.y + 0.5f * deltaQuaternion.value.y,
				z: m_Pose.Rotation.value.z + 0.5f * deltaQuaternion.value.z,
				w: m_Pose.Rotation.value.w + 0.5f * deltaQuaternion.value.w);
			quaternion = math.normalize(quaternion);

			m_Pose = Computations.SetRotation(m_Pose, quaternion);
		}
	}
}
