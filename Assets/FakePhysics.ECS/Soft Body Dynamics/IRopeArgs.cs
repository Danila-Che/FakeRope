namespace FakePhysics.ECS.SoftBodyDynamics
{
	public interface IRopeArgs
	{
		float SpanDistance { get; }

		float Drag { get; }

		float Mass { get; }
	}
}
