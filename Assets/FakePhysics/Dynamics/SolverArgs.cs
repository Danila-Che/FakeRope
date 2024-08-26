using System;
using Unity.Mathematics;
using UnityEngine;

namespace FakePhysics.Dynamics
{
	[Serializable]
	public class SolverArgs
	{
		[SerializeField] private float3 m_GravitationalAcceleration = Physics.gravity;
		[Min(1)]
		[SerializeField] private int m_SubstepIteractionsNumber = 1;
		[Min(1)]
		[SerializeField] private int m_SolvePositionIteractionsNumber = 1;
		[Min(1)]
		[SerializeField] private int m_SolverCollisionIteractionNumber = 1;

		public float3 GravitationalAcceleration => m_GravitationalAcceleration;
		public int SubstepIteractionsNumber => m_SubstepIteractionsNumber;
		public int SolvePositionIteractionsNumber => m_SolvePositionIteractionsNumber;
		public int SolverCollisionIteractionNumber => m_SolverCollisionIteractionNumber;
	}
}
