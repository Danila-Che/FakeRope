using Unity.Entities;
using UnityEngine;

namespace FakePhysics.Sample
{
	internal struct MyComponent : IComponentData
	{
		public int Value;
	}

	internal struct UnionComponent : IComponentData
	{
		public Entity FirstEntity;
		public Entity SecondEntity;
	}

	internal partial class FirstSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Dependency = Entities.ForEach((ref MyComponent component) =>
			{
				Debug.Log($"First {component.Value}");

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
				Debug.Log($"Second {component.Value}");

				component.Value += 42;
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
			}).Run();
		}
	}

	internal class ECSTestComponent : MonoBehaviour
	{
		private World m_World;

		private FirstSystem m_FirstSystem;
		private SecondSystem m_SecondSystem;
		private UnionSystem m_UnionSystem;

		private void OnEnable()
		{
			m_World = new World("Test World");

			var first = m_World.EntityManager.CreateEntity(typeof(MyComponent));
			var second = m_World.EntityManager.CreateEntity(typeof(MyComponent));
			var union = m_World.EntityManager.CreateEntity(typeof(UnionComponent));

			m_World.EntityManager.SetComponentData(first, new MyComponent());
			m_World.EntityManager.SetComponentData(second, new MyComponent());
			m_World.EntityManager.SetComponentData(union, new UnionComponent
			{
				FirstEntity = first,
				SecondEntity = second,
			});

			m_FirstSystem = m_World.CreateSystemManaged<FirstSystem>();
			m_SecondSystem = m_World.CreateSystemManaged<SecondSystem>();
			m_UnionSystem = m_World.CreateSystemManaged<UnionSystem>();
		}

		private void Update()
		{
			m_FirstSystem.Update();
			m_UnionSystem.Update();
			m_SecondSystem.Update();
		}
	}
}
