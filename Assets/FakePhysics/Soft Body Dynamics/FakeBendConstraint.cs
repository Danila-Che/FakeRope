using FakePhysics.Utilities;

namespace FakePhysics.SoftBodyDynamics
{
	public struct FakeBendConstraint
	{
		public int Index0;
		public int Index1;
		public int Index2;
		public float RestAngle;

		public FakeBendConstraint(int index0, int index1, int index2, float restAngle)
		{
			Index0 = index0;
			Index1 = index1;
			Index2 = index2;
			RestAngle = restAngle;
		}

		public override readonly string ToString()
		{
			return CommonUtilities.ToNumberFormat(
				"BendConstraint({0}, {1} {2}) rest angle {3}",
				Index0,
				Index1,
				Index2,
				RestAngle);
		}
	}
}
