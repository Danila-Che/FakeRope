using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics
{
    public class DistanceConstraintSystem : FakeSystemBase
    {
        [BurstCompile]
        private struct DistanceConstraintJob : IJob
		{
			[ReadOnly]
            public FakeChunksCollection<FakeDistanceConstraint> DistanceConstraints;
			public FakeChunksCollection<FakeParticle> Particles;

			public void Execute()
			{
				for (int i = 0; i < DistanceConstraints.Length; i++)
				{
					var constraint = DistanceConstraints[i];

					var particle0 = Particles[constraint.Particle0];
					var particle1 = Particles[constraint.Particle1];

					var correction = SoftBodyComputations.CalculateDistanceConstraintCorrection(particle0, particle1, constraint.Distance);

					var w = particle0.InverseMass + particle1.InverseMass;

					var k0 = particle0.InverseMass / w;
					var k1 = particle1.InverseMass / w;

					particle0.Position -= k0 * correction;
					particle1.Position += k1 * correction;

					Particles[constraint.Particle0] = particle0;
					Particles[constraint.Particle1] = particle1;
				}
			}
		}

        protected override void OnCreate()
		{
			RequireAsPrimary<FakeDistanceConstraint>();
			RequireAsSecondary<FakeParticle>();
		}

        protected override void OnUpdate()
        {
            Schedule(new DistanceConstraintJob
            {
                DistanceConstraints = Get<FakeDistanceConstraint>(),
                Particles = Get<FakeParticle>(),
            });
        }
    }
}
