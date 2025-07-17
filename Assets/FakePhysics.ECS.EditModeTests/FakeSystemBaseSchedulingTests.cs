using System.Diagnostics;
using FakePhysics.ECS.Utilities;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace FakePhysics.ECS.EditModeTests
{
    [TestFixture]
    public class FakeSystemBaseSchedulingTests
    {
        private struct FirstStub
        {
            public int Value;
        }

        public struct SecondStub
        {
            public int Value;
        }

        public struct FirstConstraint
        {
            public FakeEntity FirstStubEntity;
            public FakeEntity SecondStubEntity;
        }

        public struct SecondConstraint
        {
            public FakeEntity FirstStubEntity; 
            public FakeEntity SecondStubEntity; 
        }

        private class FirstParallelSystem : FakeSystemBase
        {
            public int Value;

            private struct StubJob : IJobParallelFor
            {
                public FakeChunksCollection<FirstStub> StubComponents;
                public int Value;

                public void Execute(int index)
                {
                    var component = StubComponents[index];

                    component.Value = Value;

                    StubComponents[index] = component;
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<FirstStub>();
            }

            protected override void OnUpdate()
            {
                ScheduleParallel(new StubJob
                {
                    StubComponents = Get<FirstStub>(),
                    Value = Value,
                });
            }
        }

        private class SecondParallelSystem : FakeSystemBase
        {
            public int Value;

            private struct StubJob : IJobParallelFor
            {
                public FakeChunksCollection<SecondStub> StubComponents;
                public int Value;

                public void Execute(int index)
                {
                    var component = StubComponents[index];

                    component.Value = Value;

                    StubComponents[index] = component;
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<SecondStub>();
            }

            protected override void OnUpdate()
            {
                ScheduleParallel(new StubJob
                {
                    StubComponents = Get<SecondStub>(),
                    Value = Value,
                });
            }
        }

        private class FirstSyncSystem : FakeSystemBase
        {
            public int Value;

            private struct StubJob : IJob
            {
                [ReadOnly] public FakeChunksCollection<FirstConstraint> StubConstraintComponents;
                public FakeChunksCollection<FirstStub> StubComponents;
                public int Value;

                public void Execute()
                {
                    for (int i = 0; i < StubConstraintComponents.Length; i++)
                    {
                        var constraint = StubConstraintComponents[i];
                        var firstComponent = StubComponents[constraint.FirstStubEntity];
                        var secondComponent = StubComponents[constraint.SecondStubEntity];
                        
                        firstComponent.Value = Value;
                        secondComponent.Value = Value;

                        StubComponents[constraint.FirstStubEntity] = firstComponent;
                        StubComponents[constraint.SecondStubEntity] = secondComponent;
                    }
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<FirstConstraint>();
                RequireAsSecondary<FirstStub>();
            }

            protected override void OnUpdate()
            {
                Schedule(new StubJob
                {
                    StubConstraintComponents = Get<FirstConstraint>(),
                    StubComponents = Get<FirstStub>(),
                    Value = Value
                });
            }
        }

        private class SecondSyncSystem : FakeSystemBase
        {
            public int Value;

            private struct StubJob : IJob
            {
                [ReadOnly] public FakeChunksCollection<SecondConstraint> StubConstraintComponents;
                public FakeChunksCollection<FirstStub> FirstStubComponents;
                public FakeChunksCollection<SecondStub> SecondStubComponents;
                public int Value;

                public void Execute()
                {
                    for (int i = 0; i < StubConstraintComponents.Length; i++)
                    {
                        var constraint = StubConstraintComponents[i];
                        var firstComponent = FirstStubComponents[constraint.FirstStubEntity];
                        var secondComponent = SecondStubComponents[constraint.SecondStubEntity];
                        
                        firstComponent.Value = Value;
                        secondComponent.Value = Value;

                        FirstStubComponents[constraint.FirstStubEntity] = firstComponent;
                        SecondStubComponents[constraint.SecondStubEntity] = secondComponent;
                    }
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<SecondConstraint>();
                RequireAsSecondary<FirstStub>();
                RequireAsSecondary<SecondStub>();
            }

            protected override void OnUpdate()
            {
                Schedule(new StubJob
                {
                    StubConstraintComponents = Get<SecondConstraint>(),
                    FirstStubComponents = Get<FirstStub>(),
                    SecondStubComponents = Get<SecondStub>(),
                    Value = Value
                });
            }
        }

        private class SyncSystemFor : FakeSystemBase
        {
            public int Value;

            private struct StubJob : IJobFor
            {
                [ReadOnly] public FakeChunksCollection<FirstConstraint> StubConstraintComponents;
                public FakeChunksCollection<FirstStub> StubComponents;
                public int Value;

                public void Execute(int index)
                {
                    var constraint = StubConstraintComponents[index];
                    var firstComponent = StubComponents[constraint.FirstStubEntity];
                    var secondComponent = StubComponents[constraint.SecondStubEntity];
                    
                    firstComponent.Value = Value;
                    secondComponent.Value = Value;

                    StubComponents[constraint.FirstStubEntity] = firstComponent;
                    StubComponents[constraint.SecondStubEntity] = secondComponent;
                }
            }

            protected override void OnCreate()
            {
                RequireAsPrimary<FirstConstraint>();
                RequireAsSecondary<FirstStub>();
            }

            protected override void OnUpdate()
            {
                ScheduleFor(new StubJob
                {
                    StubConstraintComponents = Get<FirstConstraint>(),
                    StubComponents = Get<FirstStub>(),
                    Value = Value
                });
            }
        }

        [Test]
        public void Test_ScheduleSyncSystem()
        {
            using var world = new FakeWorld();

            var firstEntity = world.CreateEntity(new FirstStub());
            var secondEntity = world.CreateEntity(new FirstStub());
            var StubConstraintComponent = world.CreateEntity(new FirstConstraint
            {
                FirstStubEntity = firstEntity,
                SecondStubEntity = secondEntity,
            });

            Assert.That(world.Count<FirstStub>(), Is.EqualTo(2));
            Assert.That(world.Count<FirstConstraint>(), Is.EqualTo(1));

            var parallelSystem = world.CreateSystem<FirstParallelSystem>();
            var syncSystem = world.CreateSystem<FirstSyncSystem>();

            parallelSystem.Value = 1;
            parallelSystem.Update();

            syncSystem.Value = 2;
            syncSystem.Update();

            syncSystem.Value = 42;
            syncSystem.Update();

            Assert.That(world.GetComponent<FirstStub>(firstEntity).Value, Is.EqualTo(42));
            Assert.That(world.GetComponent<FirstStub>(secondEntity).Value, Is.EqualTo(42));
        }

        [Test]
        public void Test_ScheduleSyncSystemFor()
        {
            using var world = new FakeWorld();

            var firstEntity = world.CreateEntity(new FirstStub());
            var secondEntity = world.CreateEntity(new FirstStub());
            var StubConstraintComponent = world.CreateEntity(new FirstConstraint
            {
                FirstStubEntity = firstEntity,
                SecondStubEntity = secondEntity,
            });

            Assert.That(world.Count<FirstStub>(), Is.EqualTo(2));
            Assert.That(world.Count<FirstConstraint>(), Is.EqualTo(1));

            var parallelSystem = world.CreateSystem<FirstParallelSystem>();
            var syncSystem = world.CreateSystem<SyncSystemFor>();

            parallelSystem.Value = 1;
            parallelSystem.Update();

            syncSystem.Value = 2;
            syncSystem.Update();

            syncSystem.Value = 42;
            syncSystem.Update();

            Assert.That(world.GetComponent<FirstStub>(firstEntity).Value, Is.EqualTo(42));
            Assert.That(world.GetComponent<FirstStub>(secondEntity).Value, Is.EqualTo(42));
        }

        [Test]
        public void Test_ScheduleSyncSystemWithThreeDependencies()
        {
            using var world = new FakeWorld();

            var entity0 = world.CreateEntity(new FirstStub());
            var entity1 = world.CreateEntity(new SecondStub());
            var StubConstraintComponent = world.CreateEntity(new SecondConstraint
            {
                FirstStubEntity = entity0,
                SecondStubEntity = entity1,
            });

            Assert.That(world.Count<FirstStub>(), Is.EqualTo(1));
            Assert.That(world.Count<SecondStub>(), Is.EqualTo(1));
            Assert.That(world.Count<SecondConstraint>(), Is.EqualTo(1));

            var system0 = world.CreateSystem<FirstParallelSystem>();
            var system1 = world.CreateSystem<SecondParallelSystem>();
            var syncSystem = world.CreateSystem<SecondSyncSystem>();

            system0.Value = 1;
            system0.Update();

            system1.Value = 2;
            system0.Update();

            syncSystem.Value = 3;
            syncSystem.Update();

            syncSystem.Value = 42;
            syncSystem.Update();

            Assert.That(world.GetComponent<FirstStub>(entity0).Value, Is.EqualTo(42));
            Assert.That(world.GetComponent<SecondStub>(entity1).Value, Is.EqualTo(42));
        }

        [Test]
        public void Test_ScheduleChainOfSystems()
        {
            using var world = new FakeWorld();

            var firstEntity = world.CreateEntity(new FirstStub());
            var secondEntity = world.CreateEntity(new FirstStub());
            var thirdEntity = world.CreateEntity(new SecondStub());
            var firstConstraint = world.CreateEntity(new FirstConstraint
            {
                FirstStubEntity = firstEntity,
                SecondStubEntity = secondEntity,
            });
            var secondConstraint = world.CreateEntity(new SecondConstraint
            {
                FirstStubEntity = firstEntity,
                SecondStubEntity = thirdEntity,
            });

            Assert.That(world.Count<FirstStub>(), Is.EqualTo(2));
            Assert.That(world.Count<SecondStub>(), Is.EqualTo(1));
            Assert.That(world.Count<FirstConstraint>(), Is.EqualTo(1));
            Assert.That(world.Count<SecondConstraint>(), Is.EqualTo(1));

            var firstParallelSystem = world.CreateSystem<FirstParallelSystem>();
            var secondParallelSystem = world.CreateSystem<SecondParallelSystem>();
            var firstSyncSystem = world.CreateSystem<FirstSyncSystem>();
            var secondSyncSystem = world.CreateSystem<SecondSyncSystem>();

            firstParallelSystem.Value = 1;
            firstParallelSystem.Update();

            secondParallelSystem.Value = 2;
            secondParallelSystem.Update();

            firstSyncSystem.Value = 3;
            firstSyncSystem.Update();

            secondSyncSystem.Value = 42;
            secondSyncSystem.Update();

            Assert.That(world.GetComponent<FirstStub>(firstEntity).Value, Is.EqualTo(42));
            Assert.That(world.GetComponent<FirstStub>(secondEntity).Value, Is.EqualTo(3));
            Assert.That(world.GetComponent<SecondStub>(thirdEntity).Value, Is.EqualTo(42));
        }
    }
}
