using FakePhysics.ECS.Utilities;
using NUnit.Framework;
using Unity.Jobs;

namespace FakePhysics.ECS.EditModeTests
{
    [TestFixture]
    public class FakeSystemBaseTests
    {
        private struct Stub
        {
            public int Value;
        }

        private class ValidSystemStub : FakeSystemBase
        {
            public int Value;
            public bool WasUpdated = false;

            private struct Job : IJobParallelFor
            {
                public int Value;
                public FakeChunksCollection<Stub> Stubs;

                public void Execute(int index)
                {
                    var stub = Stubs[index];

                    stub.Value = Value;

                    Stubs[index] = stub;
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<Stub>();
            }

            protected override void OnUpdate()
            {
                WasUpdated = true;

                ScheduleParallel(new Job
                {
                    Value = Value,
                    Stubs = Get<Stub>(),
                });
            }
        }

        private class InvalidSystemStub : FakeSystemBase
        {
            public bool WasUpdated = false;

            protected override void OnCreate() { }

            protected override void OnUpdate()
            {
                WasUpdated = true;
            }
        }

        private class SystemStub : FakeSystemBase
        {
            public int Value;

            private struct Job : IJob
            {
                public int Value;
                public FakeChunksCollection<Stub> Stubs;

                public void Execute()
                {
                    for (int i = 0; i < Stubs.Length; i++)
                    {
                        var stub = Stubs[i];

                        stub.Value = Value;

                        Stubs[i] = stub;
                    }
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<Stub>();
            }

            protected override void OnUpdate()
            {
                Schedule(new Job
                {
                    Value = Value,
                    Stubs = Get<Stub>(),
                });
            }
        }

        [Test]
        public void Test_CreateSystem()
        {
            using var world = new FakeWorld();

            var system = world.CreateSystem<ValidSystemStub>();

            Assert.That(system, Is.Not.Null);
            Assert.That(system, Is.EqualTo(world.GetSystem<ValidSystemStub>()));
        }

        [Test]
        public void Test_SystemRequirement()
        {
            using var world = new FakeWorld();

            var system = world.CreateSystem<ValidSystemStub>();

            Assert.That(system.IsValid, Is.False);

            var entity = world.CreateEntity(new Stub());

            Assert.That(system.IsValid, Is.True);
        }

        [Test]
        public void Test_DontUpdateSystemIfSystemInvalid()
        {
            using var world = new FakeWorld();

            var system = world.CreateSystem<InvalidSystemStub>();

            Assert.That(system.IsValid, Is.False);
            Assert.That(system.WasUpdated, Is.False);

            system.Update();

            Assert.That(system.IsValid, Is.False);
            Assert.That(system.WasUpdated, Is.False);
        }

        [Test]
        public void Test_DontUpdateSystemIfThereNoEntities()
        {
            using var world = new FakeWorld();

            var system = world.CreateSystem<ValidSystemStub>();

            Assert.That(system.IsValid, Is.False);
            Assert.That(system.WasUpdated, Is.False);

            system.Update();

            Assert.That(system.IsValid, Is.False);
            Assert.That(system.WasUpdated, Is.False);
        }

        [Test]
        public void Test_ScheduleParallelSystem()
        {
            using var world = new FakeWorld();

            var entity = world.CreateEntity(new Stub());
            var system = world.CreateSystem<ValidSystemStub>();

            system.Value = 42;
            system.Update();

            Assert.That(world.GetComponent<Stub>(entity).Value, Is.EqualTo(42));
        }

        [Test]
        public void Test_ScheduleSyncSystem()
        {
            using var world = new FakeWorld();

            var entity = world.CreateEntity(new Stub());
            var system = world.CreateSystem<SystemStub>();

            system.Value = 42;
            system.Update();

            Assert.That(world.GetComponent<Stub>(entity).Value, Is.EqualTo(42));
        }
    }
}
