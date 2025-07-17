using FakePhysics.ECS.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace FakePhysics.ECS.SoftBodyDynamics
{
    public class BendConstraintSystem : FakeSystemBase
    {
        [BurstCompile]
		private struct BendConstraintSolver : IJob
		{
			[ReadOnly] public FakeChunksCollection<FakeBendConstraint> BendConstraints;
			public FakeChunksCollection<FakeParticle> Particles;

			public void Execute()
			{
				for (int i = 0; i < BendConstraints.Length; i++)
				{
					var constraint = BendConstraints[i];

					var particle0 = Particles[constraint.Particle0];
					var particle1 = Particles[constraint.Particle1];
					var particle2 = Particles[constraint.Particle2];

					var correction = SoftBodyComputations.CalculateBendConstraintCorrection(particle0.Position, particle1.Position, particle2.Position);

					var w = particle0.InverseMass + 2f * particle1.InverseMass + particle2.InverseMass;

					var k0 = 2f * particle0.InverseMass / w;
					var k1 = 4f * particle1.InverseMass / w;
					var k2 = 2f * particle2.InverseMass / w;

					particle0.Position += k0 * constraint.Stiffness * correction;
					particle1.Position -= k1 * constraint.Stiffness * correction;
					particle2.Position += k2 * constraint.Stiffness * correction;

					Particles[constraint.Particle0] = particle0;
					Particles[constraint.Particle1] = particle1;
					Particles[constraint.Particle2] = particle2;
				}
			}
		}

		protected override void OnCreate()
		{
			RequireAsPrimary<FakeBendConstraint>();
			RequireAsSecondary<FakeParticle>();
		}

        protected override void OnUpdate()
        {
            Schedule(new BendConstraintSolver
            {
                BendConstraints = Get<FakeBendConstraint>(),
                Particles = Get<FakeParticle>(),
            });
        }
    }
}
