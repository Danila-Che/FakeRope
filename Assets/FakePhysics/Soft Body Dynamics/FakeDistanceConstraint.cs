using FakePhysics.Utilities;

namespace FakePhysics.SoftBodyDynamics
{
	public struct FakeDistanceConstraint
	{
		public int Index0;
		public int Index1;
		public float Distance;

		public FakeDistanceConstraint(int index0, int index1, float distance)
		{
			Index0 = index0;
			Index1 = index1;
			Distance = distance;
		}

		public override readonly string ToString()
		{
			return CommonUtilities.ToNumberFormat("FakeDistanceConstraint({0}, {1}) distance {2}", Index0, Index1, Distance);
		}
	}
}
