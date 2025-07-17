using System;
using System.Collections.Generic;
using System.Linq;

namespace FakePhysics.ECS.Utilities
{
    public class FakeWorld : IDisposable
    {
        public interface IComponentContainer : IDisposable
        {
            int Length { get; }
        }

        public interface IComponentContainer<T> : IComponentContainer
            where T : unmanaged
        {
            public FakeChunksCollection<T> Components { get; }
        }

        private readonly Dictionary<Type, IFakeComponentContainer> m_Containers; // Type is type of a component

        private readonly List<FakeSystemBase> m_Systems;

        public FakeWorld()
        {
            m_Containers = new Dictionary<Type, IFakeComponentContainer>();
            m_Systems = new List<FakeSystemBase>();
        }

        public int ContainersCount => m_Containers.Count;

        public void Dispose()
        {
            m_Systems.Clear();

            foreach (var container in m_Containers.Values)
            {
                container.Dispose();
            }
        }

        public int Count<T>()
            where T : unmanaged
        {
            return m_Containers[typeof(T)].Length;
        }

        public FakeEntity CreateEntity<T>(T component)
            where T : unmanaged
        {
            return GetOrCreateContainer<T>().Create(component);
        }

        public T GetComponent<T>(FakeEntity entity)
            where T : unmanaged
        {
            return GetContiainer<T>().Get(entity);
        }

        public void SetComponent<T>(FakeEntity entity, T component)
            where T : unmanaged
        {
            GetContiainer<T>().Set(entity, component);
        }

        public T CreateSystem<T>()
            where T : FakeSystemBase, new()
        {
            if (m_Systems.Any(system => system.GetType() is T))
            {
                return m_Systems.First(system => system.GetType() is T) as T;
            }

            var system = new T();

            m_Systems.Add(system);
            system.Init(world: this);

            return system;
        }

        public T GetSystem<T>()
            where T : FakeSystemBase
        {
            foreach (var system in m_Systems)
            {
                if (system is T)
                {
                    return system as T;
                }
            }

            return null;
        }

        public bool Has<T>()
        {
            return m_Containers.ContainsKey(typeof(T));
        }
        
        public FakeComponentContainer<T> RequireContainer<T>()
            where T : unmanaged
        {
            return GetOrCreateContainer<T>();
        }

        private FakeComponentContainer<T> GetContiainer<T>()
            where T : unmanaged
        {
            return m_Containers[typeof(T)] as FakeComponentContainer<T>;
        }

        private FakeComponentContainer<T> GetOrCreateContainer<T>()
            where T : unmanaged
        {
            if (m_Containers.TryGetValue(typeof(T), out var result))
            {
                return result as FakeComponentContainer<T>;
            }

            return CreateContainer<T>();
        }

        private FakeComponentContainer<T> CreateContainer<T>()
            where T : unmanaged
        {
            var result = new FakeComponentContainer<T>();

            m_Containers[typeof(T)] = result;
            
            return result;
        }
    }
}
