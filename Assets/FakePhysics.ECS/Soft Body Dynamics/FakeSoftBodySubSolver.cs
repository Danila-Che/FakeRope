using FakePhysics.ECS.Dynamics;
using FakePhysics.ECS.RigidBodyDynamics;
using FakePhysics.ECS.SoftBodyDynamics.Systems;
using FakePhysics.ECS.Utilities;
using Unity.Mathematics;

namespace FakePhysics.ECS.SoftBodyDynamics
{
	public class FakeSoftBodySubSolver : IFakeSubSolver, IFakeDynamicSubSolver, IFakeConstrainedSubSolver
	{
		private FakeWorld m_World;

		private BeginStepSystem m_BeginStepSystem;
		private StepSystem m_StepSystem;
		private EndStepSystem m_EndStepSystem;
		private ApplyAccelerationSystem m_ApplyAccelerationSystem;
		private ApplyDragSystem m_ApplyDragSystem;

		private AttachmentConstraintSystem m_AttachmentConstraintSystem;
		private DistanceConstraintSystem m_DistanceConstraint;
		private BendConstraintSystem m_BendConstraintSystem;

		public void Init(FakeWorld world)
		{
			m_World = world;

			m_BeginStepSystem = m_World.CreateSystem<BeginStepSystem>();
			m_StepSystem = m_World.CreateSystem<StepSystem>();
			m_EndStepSystem = m_World.CreateSystem<EndStepSystem>();
			m_ApplyAccelerationSystem = m_World.CreateSystem<ApplyAccelerationSystem>();
			m_ApplyDragSystem = m_World.CreateSystem<ApplyDragSystem>();

			m_AttachmentConstraintSystem = m_World.CreateSystem<AttachmentConstraintSystem>();
			m_DistanceConstraint = m_World.CreateSystem<DistanceConstraintSystem>();
			m_BendConstraintSystem = m_World.CreateSystem<BendConstraintSystem>();
		}

		public FakeEntity RequireEntity(FakeParticle particle)
		{
			return m_World.CreateEntity(particle);
		}

		public FakeEntity RequireEntity(FakeDistanceConstraint constraint)
		{
			return m_World.CreateEntity(constraint);
		}

		public FakeEntity RequireEntity(FakeBendConstraint constraint)
		{
			return m_World.CreateEntity(constraint);
		}

		public FakeEntity RequireEntity(FakeParticleJoint joint)
		{
			return m_World.CreateEntity(joint);
		}

		public FakeParticle GetParticle(FakeEntity entity)
		{
			return m_World.GetComponent<FakeParticle>(entity);
		}

		public FakeRigidBody GetRigidBody(FakeEntity entity)
		{
			return m_World.GetComponent<FakeRigidBody>(entity);
		}

		public void BeginStep()
		{
			m_BeginStepSystem.Update();
		}

		public void Step(float deltaTime)
		{
			m_StepSystem.DeltaTime = deltaTime;

			m_StepSystem.Update();
		}

		public void EndStep(float deltaTime)
		{
			m_EndStepSystem.DeltaTime = deltaTime;

			m_EndStepSystem.Update();
		}

		public void ApplyAcceleration(float deltaTime, float3 acceleration)
		{
			m_ApplyAccelerationSystem.DeltaTime = deltaTime;
			m_ApplyAccelerationSystem.Acceleration = acceleration;

			m_ApplyAccelerationSystem.Update();
		}

		public void ApplyDrag(float deltaTime)
		{
			m_ApplyDragSystem.DeltaTime = deltaTime;

			m_ApplyDragSystem.Update();
		}

		public void SolveConstraints(float deltaTime)
		{
			m_AttachmentConstraintSystem.DeltaTime = deltaTime;

			m_AttachmentConstraintSystem.Update();
			m_DistanceConstraint.Update();
			m_BendConstraintSystem.Update();
		}
	}
}
