using FakePhysics.Controllers;
using UnityEngine;

namespace FakePhysics.Sample
{
	public class RopeChangeLengthDemo : MonoBehaviour
	{
		[SerializeField] private FakeRopeCursor m_RopeCursor;

		private void Update()
		{
			if (Input.GetKey(KeyCode.DownArrow))
			{
				m_RopeCursor.Lower();
			}
			else
			{
				m_RopeCursor.StopLower();
			}

			if (Input.GetKey(KeyCode.UpArrow))
			{
				m_RopeCursor.Lift();
			}
			else
			{
				m_RopeCursor.StopLift();
			}
		}
	}
}
