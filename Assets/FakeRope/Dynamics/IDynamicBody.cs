using Unity.Mathematics;

namespace Fake.Dynamics
{
    public interface IDynamicBody
    {
        void BeginStep();

        void Step(float deltaTime);

        void EndStep(float deltaTime);

		void ApplyAcceleration(float deltaTime, float3 acceleration);

		void ApplyDrag(float deltaTime);
	}
}
