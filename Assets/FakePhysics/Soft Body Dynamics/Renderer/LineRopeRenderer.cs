using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FakePhysics.SoftBodyDynamics.Renderer
{
	[RequireComponent(typeof(LineRenderer))]
	internal class LineRopeRenderer : MonoBehaviour, IRenderer
	{
		[SerializeField] private bool m_UseCustomWidth;
		[Min(0f)]
		[SerializeField] private float m_CustomWidth = 0.1f;

		private LineRenderer m_LineRenderer;
		private RopeArgs m_RopeArgs;

		private float RopeWidth => m_UseCustomWidth ? m_CustomWidth : 2f * m_RopeArgs.Radius;

		public void Init(RopeArgs ropeArgs)
		{
			m_RopeArgs = ropeArgs;
			m_LineRenderer = GetComponent<LineRenderer>();
		}

		public void Draw(NativeArray<FakeParticle>.ReadOnly particles)
		{
			m_LineRenderer.startWidth = RopeWidth;
			m_LineRenderer.endWidth = RopeWidth;
			m_LineRenderer.positionCount = particles.Length;

			for (int i = 0; i < particles.Length; i++)
			{
				m_LineRenderer.SetPosition(i, particles[i].Position);
			}
		}

		public void Draw(FakeParticle[] particles)
		{
			m_LineRenderer.startWidth = RopeWidth;
			m_LineRenderer.endWidth = RopeWidth;
			m_LineRenderer.positionCount = particles.Length;

			for (int i = 0; i < particles.Length; i++)
			{
				m_LineRenderer.SetPosition(i, particles[i].Position);
			}
		}

		public void Draw(List<FakeParticle> particles)
		{
			m_LineRenderer.startWidth = RopeWidth;
			m_LineRenderer.endWidth = RopeWidth;
			m_LineRenderer.positionCount = particles.Count;

			for (int i = 0; i < particles.Count; i++)
			{
				m_LineRenderer.SetPosition(i, particles[i].Position);
			}
		}
	}
}
