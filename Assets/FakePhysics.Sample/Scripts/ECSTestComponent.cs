using Unity.Entities;
using UnityEngine;

namespace FakePhysics.Sample
{
	internal struct MyComponent : IComponentData
	{
		public int Value;
	}

	internal struct AnotherComponent : IComponentData
	{
		public int Value;
	}

	internal struct UnionComponent : IComponentData
	{
		public Entity FirstEntity;
		public Entity SecondEntity;
		public Entity ThirdEntity;
	}

	internal partial class FirstSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Dependency = Entities.ForEach((ref MyComponent component) =>
			{
				component.Value = 0;
			}).ScheduleParallel(Dependency);
		}
	}

	internal partial class SecondSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Dependency = Entities.ForEach((ref MyComponent component) =>
			{
				component.Value += 42;
			}).ScheduleParallel(Dependency);
		}
	}

	internal partial class ThirdSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Dependency = Entities.ForEach((ref AnotherComponent component) =>
			{
				component.Value = 0;
			}).ScheduleParallel(Dependency);
		}
	}

	internal partial class UnionSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			var entityManager = World.EntityManager;

			Entities.ForEach((in UnionComponent union) =>
			{
				if (entityManager.HasComponent<MyComponent>(union.FirstEntity))
				{
					var firstComponent = entityManager.GetComponentData<MyComponent>(union.FirstEntity);
					firstComponent.Value += 1;
					entityManager.SetComponentData(union.FirstEntity, firstComponent);
				}

				if (entityManager.HasComponent<MyComponent>(union.SecondEntity))
				{
					var secondComponent = entityManager.GetComponentData<MyComponent>(union.SecondEntity);
					secondComponent.Value += 2;
					entityManager.SetComponentData(union.SecondEntity, secondComponent);
				}

				if (entityManager.HasComponent<AnotherComponent>(union.ThirdEntity))
				{
					var thirdEntity = entityManager.GetComponentData<AnotherComponent>(union.ThirdEntity);
					thirdEntity.Value--;
					entityManager.SetComponentData(union.SecondEntity, thirdEntity);
				}
			}).Run();
		}
	}

	internal class ECSTestComponent : MonoBehaviour
	{
		private World m_World;

		private FirstSystem m_FirstSystem;
		private SecondSystem m_SecondSystem;
		private ThirdSystem m_ThirdSystem;
		private UnionSystem m_UnionSystem;

		private Entity m_First;
		private Entity m_Second;
		private Entity m_Third;

		private void OnEnable()
		{
			m_World = new World("Test World");

			m_First = m_World.EntityManager.CreateEntity(typeof(MyComponent));
			m_Second = m_World.EntityManager.CreateEntity(typeof(MyComponent));
			m_Third = m_World.EntityManager.CreateEntity(typeof(AnotherComponent));
			var union = m_World.EntityManager.CreateEntity(typeof(UnionComponent));

			m_World.EntityManager.SetComponentData(m_First, new MyComponent());
			m_World.EntityManager.SetComponentData(m_Second, new MyComponent());
			m_World.EntityManager.SetComponentData(m_Third, new AnotherComponent());
			m_World.EntityManager.SetComponentData(union, new UnionComponent
			{
				FirstEntity = m_First,
				SecondEntity = m_Second,
				ThirdEntity = m_Third,
			});

			m_FirstSystem = m_World.CreateSystemManaged<FirstSystem>();
			m_SecondSystem = m_World.CreateSystemManaged<SecondSystem>();
			m_UnionSystem = m_World.CreateSystemManaged<UnionSystem>();
			m_ThirdSystem = m_World.CreateSystemManaged<ThirdSystem>();
		}

		private void Update()
		{
			var first = m_World.EntityManager.GetComponentData<MyComponent>(m_First).Value;
			var second = m_World.EntityManager.GetComponentData<MyComponent>(m_Second).Value;
			var third = m_World.EntityManager.GetComponentData<AnotherComponent>(m_Third).Value;

			Debug.Log($"> {first} {second} {third}");

			for (int i = 0; i < 100; i++)
			{
				m_FirstSystem.Update();
				m_ThirdSystem.Update();
			}

			m_UnionSystem.Update();
			m_SecondSystem.Update();
		}
	}
}
