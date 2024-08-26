using Fake.CollisionDetection;
using Fake.Dynamics;
using Fake.Utilities;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Fake.RigidBodyDynamics
{
	public class FakeRigidBody : IDynamicBody, ICollidingBody
	{
		private const float k_MaxRotationPerSubstep = 0.5f;

		private readonly bool m_IsKinematic;
		private readonly FakeBoxCollider m_BoxCollider;

		private FakePose m_Pose;
		private FakePose m_PreviousPose;

		private float3 m_Velocity;
		private float3 m_AngularVelocity;
		private readonly float3 m_Drag;

		private readonly float m_InverseMass;
		private readonly float3 m_InverseInertiaTensor;

		public FakeRigidBody(UnityEngine.Rigidbody rigidbody, FakeBoxCollider boxCollider)
			: this(rigidbody.mass, boxCollider)
		{
			m_Pose = new FakePose(rigidbody.position, rigidbody.rotation);
			m_PreviousPose = m_Pose;

			m_IsKinematic = rigidbody.isKinematic;
			m_Drag = rigidbody.drag;

			m_Velocity = float3.zero;
			m_AngularVelocity = float3.zero;
		}

		private FakeRigidBody(float mass, FakeBoxCollider boxCollider)
		{
			m_BoxCollider = boxCollider;

			m_InverseMass = math.isfinite(mass) switch
			{
				true => 1.0f / mass,
				false => 0.0f,
			};

			m_InverseInertiaTensor = CollisionComputations.CalculateInverseInertiaTensor(boxCollider, mass);
		}

		public bool IsKinematic => m_IsKinematic;

		public FakePose Pose => m_Pose;

		public FakeBoxCollider BoxCollider => m_BoxCollider;

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
				x: 2.0f * deltaQuaternion.value.x / deltaTime,
				y: 2.0f * deltaQuaternion.value.y / deltaTime,
				z: 2.0f * deltaQuaternion.value.z / deltaTime);

			if (deltaQuaternion.value.w < 0.0f)
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

		public void ApplyCorrection(float3 correction, float3? position = null, bool velocityLevel = false)
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
					m_Velocity += correction * m_InverseMass;
				}
				else
				{
					m_Pose = Computations.Translate(m_Pose, correction * m_InverseMass);
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

		private void ApplyRotation(float3 rotation, float scale = 1.0f)
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
				w: 0.0f);
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
