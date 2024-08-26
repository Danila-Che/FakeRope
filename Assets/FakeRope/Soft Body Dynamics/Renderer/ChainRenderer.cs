using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Fake.SoftBodyDynamics.Renderer
{
	public class ChainRenderer : MonoBehaviour, IRenderer
	{
		[SerializeField] private Mesh m_ChainMesh;
		[SerializeField] private Material m_Material;
		[SerializeField] private Vector3 m_Scale = Vector3.one;
		[SerializeField] private Vector3 m_Rotation = Vector3.zero;
		[Min(0.01f)]
		[SerializeField] private float m_Offset = 0.01f;
		[SerializeField] private Vector3 m_WorldBound = 100.0f * Vector3.one;

		private RenderParams m_RenderParams;

		private List<Matrix4x4> m_TRS;
		private Vector3 m_Error;
		private int m_ParityBit;

		public void Init()
		{
			m_RenderParams = new RenderParams(m_Material)
			{
				worldBounds = new Bounds(Vector3.zero, m_WorldBound),
			};

			m_TRS = new List<Matrix4x4>(capacity: 100);
		}

		public void Draw(NativeArray<FakeParticle>.ReadOnly particles)
		{
			BeginDraw();

			for (int i = 0; i < particles.Length - 1; i++)
			{
				DrawSegment(
					particles[i].Position,
					particles[i + 1].Position);
			}

			EndDraw();
		}

		public void Draw(FakeParticle[] particles)
		{
			BeginDraw();

			for (int i = 0; i < particles.Length - 1; i++)
			{
				DrawSegment(
					particles[i].Position,
					particles[i + 1].Position);
			}

			EndDraw();
		}

		public void Draw(List<FakeParticle> particles)
		{
			BeginDraw();

			for (int i = 0; i < particles.Count - 1; i++)
			{
				DrawSegment(
					particles[i].Position,
					particles[i + 1].Position);
			}

			EndDraw();
		}

		private void BeginDraw()
		{
			m_TRS.Clear();
			m_Error = Vector3.zero;
			m_ParityBit = 0;
		}

		private void DrawSegment(Vector3 from, Vector3 to)
		{
			from -= m_Error;

			var vector = to - from;
			var normal = vector.normalized;
			var length = vector.magnitude;
			var count = (int)(Mathf.Ceil(length / m_Offset));

			var trs = Matrix4x4.identity;
			var lookRotation = Quaternion.LookRotation(normal) * Quaternion.Euler(m_Rotation);
			var crossRotation = lookRotation * Quaternion.Euler(0.0f, 0.0f, 90.0f);

			for (int i = 0; i < count; i++)
			{
				var position = from + i * m_Offset * normal;
				var rotation = ((i & 1) == m_ParityBit) switch
				{
					true => lookRotation,
					false => crossRotation,
				};

				trs.SetTRS(position, rotation, m_Scale);

				m_TRS.Add(trs);
			}

			m_Error = (length - count * m_Offset) * normal;
			m_ParityBit ^= count & 1;
		}

		private void EndDraw()
		{
			Graphics.RenderMeshInstanced(
				in m_RenderParams,
				m_ChainMesh,
				submeshIndex: 0,
				m_TRS);
		}
	}
}
