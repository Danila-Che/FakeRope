using FakePhysics.RigidBodyDynamics;
using FakePhysics.SoftBodyDynamics;
using FakePhysics.SoftBodyDynamics.Renderer;
using FakePhysics.Utilities;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Controllers
{
	[RequireComponent(typeof(IRenderer))]
	[DefaultExecutionOrder((int)ExecutionOrder.SoftBody)]
	public class FakeRopeController : MonoBehaviour
	{
		[Header("First Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_AnchorBody;
		[SerializeField] private float3 m_AnchorLocalAttachement;

		[Header("Second Body")]
		[SerializeField] private FakeRigidBodyControllerBase m_TargetBody;
		[SerializeField] private float3 m_TargetLocalAttachement;

		[Header("Settings")]
		[SerializeField] private FakeRope.RopeMode m_Mode = FakeRope.RopeMode.SequentialSimulation;
		[SerializeField] private RopeSettings m_RopeSettings;

		private FakeRope m_Rope;
		private IRenderer m_Renderer;
		private IRopeModifier[] m_RopeModifiers;

		private FakeSolverController m_FakeSolverController;

		public float Length => m_Rope.Length;

		private void OnEnable()
		{
			m_FakeSolverController = GetComponentInParent<FakeSolverController>();

			m_Rope = new FakeRope(CreateJoint(), m_RopeSettings.Args)
			{
				Mode = m_Mode
			};

			m_Rope.CreateFromJoint();

			m_Renderer = GetComponent<IRenderer>();
			m_Renderer.Init(m_RopeSettings.Args);

			m_RopeModifiers = GetComponentsInParent<IRopeModifier>();

			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.RegisterSolfBody(m_Rope);

				m_FakeSolverController.BeforeStep += OnStepBegin;
			}
		}

		private void OnDisable()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterSolfBody(m_Rope);

				m_FakeSolverController.BeforeStep -= OnStepBegin;
			}

			m_Rope?.Dispose();
		}

		private void OnDestroy()
		{
			if (m_FakeSolverController != null)
			{
				m_FakeSolverController.UnregisterSolfBody(m_Rope);

				m_FakeSolverController.BeforeStep -= OnStepBegin;
			}

			m_Rope?.Dispose();
		}

		private void Update()
		{
			m_Renderer.Draw(m_Rope.Particles);
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (m_AnchorBody == null || m_TargetBody == null) { return; }

			var joint = CreateJoint();

			Gizmos.color = Color.green;

			joint.RecalculateGlobalPoses(m_AnchorBody.transform, m_TargetBody.transform);

			Gizmos.DrawSphere(joint.AnchorGlobalPose.Position, 0.1f);
			Gizmos.DrawSphere(joint.TargetGlobalPose.Position, 0.1f);

			if (m_Rope != null)
			{
				var particles = m_Rope.Particles;

				for (int i = 0; i < particles.Length; i++)
				{
					Gizmos.DrawSphere(particles[i].Position, 0.05f);
				}
			}
		}

#endif

		public void SetLength(float length)
		{
			m_Rope.ChangeLength(length - m_Rope.Length);
		}

		private FakeJoint CreateJoint()
		{
			return new FakeJoint(
				m_AnchorBody.Body,
				m_TargetBody.Body,
				new FakePose(m_AnchorLocalAttachement, quaternion.identity),
				new FakePose(m_TargetLocalAttachement, quaternion.identity));
		}

		private void OnStepBegin()
		{
			Array.ForEach(m_RopeModifiers, modifier => modifier.Modify(m_Rope, Time.fixedDeltaTime));
		}
	}
}
