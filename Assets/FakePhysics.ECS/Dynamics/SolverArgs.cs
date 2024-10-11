using System;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.ECS.Dynamics
{
	[Serializable]
	public class SolverArgs
	{
		[SerializeField] private float3 m_GravitationalAcceleration = Physics.gravity;
		[Min(1)]
		[SerializeField] private int m_SubIterationsNumber = 1;

		public float3 GravitationalAcceleration => m_GravitationalAcceleration;

		public int SubIterationsNumber => m_SubIterationsNumber;
	}
}
