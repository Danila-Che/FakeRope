using FakePhysics.CollisionDetection;
using FakePhysics.EditModeTests.Utilities;
using FakePhysics.RigidBodyDynamics;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class RigidBodyTests
	{
		private Float3Comparer m_Float3Comparer;
		private Rigidbody m_RigidBody;
		private FakeBoxCollider m_FakeBoxCollider;

		[SetUp]
		public void SetUp()
		{
			m_Float3Comparer = new Float3Comparer();

			m_RigidBody = new GameObject().AddComponent<Rigidbody>();
			m_RigidBody.mass = 1f;

			m_FakeBoxCollider = new FakeBoxCollider(new float3(1f));
		}

		[Test]
		public void Test_FreezeRotationByAllAxis()
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

			var fakeRigidBody = new FakeRigidBody(m_RigidBody, m_FakeBoxCollider);

			Assert.That(fakeRigidBody.InverseInertiaTensor, Is.EqualTo(float3.zero).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_FreezeRotationByAxisX()
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeRotationX;

			var fakeRigidBody = new FakeRigidBody(m_RigidBody, m_FakeBoxCollider);

			Assert.That(fakeRigidBody.InverseInertiaTensor, Is.EqualTo(new float3(0f, 6f, 6f)).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_FreezeRotationByAxisY()
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeRotationY;

			var fakeRigidBody = new FakeRigidBody(m_RigidBody, m_FakeBoxCollider);

			Assert.That(fakeRigidBody.InverseInertiaTensor, Is.EqualTo(new float3(6f, 0f, 6f)).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_FreezeRotationByAxisZ()
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;

			var fakeRigidBody = new FakeRigidBody(m_RigidBody, m_FakeBoxCollider);

			Assert.That(fakeRigidBody.InverseInertiaTensor, Is.EqualTo(new float3(6f, 6f, 0f)).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_RotationDoesNotChange()
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

			var fakeRigidBody = new FakeRigidBody(m_RigidBody, m_FakeBoxCollider);
			var previousRotation = fakeRigidBody.Pose.Rotation;
			fakeRigidBody.ApplyCorrection(math.up(), new float3(1f));

			Assert.That(fakeRigidBody.Pose.Rotation, Is.EqualTo(previousRotation));
		}
	}
}
