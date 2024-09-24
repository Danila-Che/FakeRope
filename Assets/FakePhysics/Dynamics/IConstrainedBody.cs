namespace FakePhysics.Dynamics
{
	public interface IConstrainedBody
	{
		void SolveInnerConstraints(float deltaTime);

		void SolveOuterConstraints(float substepDeltaTime);
	}
}
