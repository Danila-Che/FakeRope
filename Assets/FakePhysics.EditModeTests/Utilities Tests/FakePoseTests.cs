using FakePhysics.EditModeTests.Utilities;
using FakePhysics.Utilities;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.EditModeTests
{
	[TestFixture]
	public class FakePoseTests
	{
		private Float3Comparer m_Float3Comparer;
		private QuaternionComparer m_QuaternionComparer;

		[SetUp]
		public void SetUp()
		{
			m_Float3Comparer = new Float3Comparer(0.0001f);
			m_QuaternionComparer = new QuaternionComparer();
		}

		[Test]
		public void Test_PoseSetRotation()
		{
			var pose = FakePose.k_Identity;
			var rotation = quaternion.EulerXZY(math.PI, math.PI, math.PI);

			pose = Computations.SetRotation(pose, rotation);

			Assert.That(pose.Position, Is.EqualTo(float3.zero).Using(m_Float3Comparer));
			Assert.That(pose.Rotation, Is.EqualTo(rotation).Using(m_QuaternionComparer));
		}

		[Test]
		public void Test_PoseTranslate()
		{
			var pose = FakePose.k_Identity;
			var vector = math.right();

			pose = Computations.Translate(pose, vector);

			Assert.That(pose.Position, Is.EqualTo(vector).Using(m_Float3Comparer));
			Assert.That(pose.Rotation, Is.EqualTo(quaternion.identity).Using(m_QuaternionComparer));
		}

		[Test]
		public void Test_PoseRotate()
		{
			var rotation = quaternion.Euler(0f, 0f, 0.5f * math.PI);
			var pose = new FakePose(float3.zero, rotation);
			var vector = math.right();
			var result = Computations.Rotate(pose, vector);

			Assert.That(result, Is.EqualTo(math.up()).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_PoseTransform()
		{
			var rotation = quaternion.Euler(0f, 0f, 0.5f * math.PI);
			var pose = new FakePose(math.right(), rotation);
			var vector = math.right();
			var result = Computations.Transform(pose, vector);

			Assert.That(result, Is.EqualTo(math.up() + math.right()).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_PoseTransformWithOtherPose()
		{
			var rotation = quaternion.Euler(0f, 0f, 0.5f * math.PI);
			var pose = new FakePose(math.right(), rotation);
			var result = Computations.Transform(pose, pose);

			Assert.That(result.Position, Is.EqualTo(math.up() + math.right()).Using(m_Float3Comparer));
			Assert.That(result.Rotation, Is.EqualTo(quaternion.Euler(0f, 0f, math.PI)).Using(m_QuaternionComparer));
		}

		[Test]
		public void Test_PoseInverseRotate()
		{
			var rotation = quaternion.Euler(0f, 0f, 0.5f * math.PI);
			var pose = new FakePose(float3.zero, rotation);
			var result = Computations.InverseRotate(pose, math.right());

			Assert.That(result, Is.EqualTo(math.down()).Using(m_Float3Comparer));
		}

		[Test]
		public void Test_PoseInverseTransform()
		{
			var rotation = quaternion.Euler(0f, 0f, 0.5f * math.PI);
			var pose = new FakePose(math.right(), rotation);
			var result = Computations.InverseTransform(pose, 2.0f * math.right());

			Assert.That(result, Is.EqualTo(math.down()).Using(m_Float3Comparer));
		}
	}
}
