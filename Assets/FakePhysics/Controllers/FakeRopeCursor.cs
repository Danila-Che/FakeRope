using FakePhysics.SoftBodyDynamics;
using System;
using UnityEngine;

namespace FakePhysics.Controllers
{
	public class FakeRopeCursor : MonoBehaviour, IRopeModifier
	{
		[Flags]
		private enum Status
		{
			None = 0b00,
			Lift = 0b01,
			Lower = 0b10,
			Stay = Lift | Lower,
		}

		[Min(0.0f)]
		[SerializeField] private float m_ChangeLengthSpeed = 1f;

		private Status m_Status;

		public void Lift()
		{
			m_Status |= Status.Lift;
		}

		public void StopLift()
		{
			m_Status &= ~Status.Lift;
		}

		public void Lower()
		{
			m_Status |= Status.Lower;
		}

		public void StopLower()
		{
			m_Status &= ~Status.Lower;
		}

		public void Modify(FakeRope rope, float deltaTime)
		{
			var lenghtDelta = m_Status switch
			{
				Status.Lift => -m_ChangeLengthSpeed,
				Status.Lower => m_ChangeLengthSpeed,
				_ => 0f,
			};

			rope.ChangeLength(lenghtDelta * deltaTime);
		}
	}
}
