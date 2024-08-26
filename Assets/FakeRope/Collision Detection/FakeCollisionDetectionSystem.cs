using Fake.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using Plane = Unity.Mathematics.Geometry.Plane;

namespace Fake.CollisionDetection
{
	public class FakeCollisionDetectionSystem
	{
		public bool TryGetCollisionPoints(
			FakeBoxCollider collider,
			FakePose pose,
			FakeBoxCollider otherCollider,
			FakePose otherPose,
			out FakeContactPair contactPair)
		{
			collider.Update(pose);
			otherCollider.Update(otherPose);

			var mayIntersects = collider.UnityBoxCollider.bounds.Intersects(otherCollider.UnityBoxCollider.bounds);

			if (mayIntersects && CollisionComputations.TryCalculatePenetration(collider, pose, otherCollider, otherPose, out contactPair))
			{
				CalculateContactPoints(collider, otherCollider, ref contactPair);
				return true;
			}

			contactPair = default;
			return false;
		}

		private void CalculateContactPoints(
			FakeBoxCollider collider,
			FakeBoxCollider otherCollider,
			ref FakeContactPair contactPair)
		{
			contactPair.Points.Clear();

			var contactPointsA = new FakeShape(4);
			var contactPointsB = new FakeShape(4);

			var point = CollisionComputations.FindFurthestPoint(collider, contactPair.Normal);
			var contactPlane = new Plane(contactPair.Normal, point);

			FindContactPoints(collider, contactPlane, ref contactPointsA);
			FindContactPoints(otherCollider, contactPlane, ref contactPointsB);

			if (contactPointsA.Count == 0)
			{
				for (int i = 0; i < contactPointsB.Count; i++)
				{
					contactPair.Points.Add(contactPointsB[i].Point);
				}

				return;
			}

			if (contactPointsB.Count == 0)
			{
				for (int i = 0; i < contactPointsA.Count; i++)
				{
					contactPair.Points.Add(contactPointsA[i].Point);
				}

				return;
			}

			//Vertex to face contact
			if (contactPointsA.Count == 1)
			{
				contactPair.Points.Add(contactPointsA[0].Point);
				return;
			}
			else if (contactPointsB.Count == 1)
			{
				contactPair.Points.Add(contactPointsB[0].Point);
				return;
			}

			CollisionComputations.CalculateAngles(contactPointsA, contactPair.Normal);
			CollisionComputations.CalculateAngles(contactPointsB, contactPair.Normal);

			contactPointsA.Sort(CollisionComputations.CompareAngle);
			contactPointsB.Sort(CollisionComputations.CompareAngle);

			using var pooledObject = ListPool<float3>.Get(out var realContactPoints);
			realContactPoints.Capacity = 4;

			CheckIntersection(contactPointsA, contactPointsB, contactPair.Normal, realContactPoints);

			if (realContactPoints.Count == 0)
			{
				CheckIntersection(contactPointsB, contactPointsA, contactPair.Normal, realContactPoints);
			}

			contactPair.Points.AddRange(realContactPoints);
		}

		private void FindContactPoints(FakeBoxCollider collider, Plane plane, ref FakeShape shape)
		{
			for (int i = 0; i < collider.WorldVertices.Length; i++)
			{
				var worldPoint = collider.WorldVertices[i];
				var distance = plane.SignedDistanceToPoint(worldPoint);

				if (math.abs(distance) > 0.005f) { continue; }

				shape.Add(worldPoint);
			}
		}

		private void CheckIntersection(
			FakeShape contactPointsA,
			FakeShape contactPointsB,
			float3 normal,
			List<float3> contactPointsBuffer)
		{
			for (int it = 0; it < contactPointsB.Count; it++)
			{
				//We assume that the first point is inside
				var passedCount = 1;

				if (CollisionComputations.IsInside(contactPointsA, contactPointsB[it].Point, normal, checkOnShape: true))
				{
					contactPointsBuffer.Add(contactPointsB[it].Point);
				}
				else
				{
					passedCount--; // = 0
				}

				var itSecondP = it + 1;
				if (itSecondP == contactPointsB.Count)
				{
					itSecondP = 0;
				}

				if (CollisionComputations.IsInside(contactPointsA, contactPointsB[itSecondP].Point, normal, checkOnShape: true))
				{
					passedCount++; // = 1 || = 2
				}
				else
				{
					passedCount--; // = 0 || = -1
				}

				//all points of current line is inside shape
				if (passedCount == 2)
				{
					continue;
				}

				//all points of current line is outside shape
				if (passedCount < 0 && !(contactPointsA.Count == 2 && contactPointsB.Count == 2))
				{
					continue;
				}

				//Check if line separate shape
				for (int jt = 0; jt < contactPointsA.Count; jt++)
				{
					int jtSecondP = jt + 1;
					if (jtSecondP == contactPointsA.Count)
					{
						jtSecondP = 0;
					}

					bool notParallel = CollisionComputations.TryGetLineIntersection(
						new FakeLine(contactPointsB[it].Point, contactPointsB[itSecondP].Point),
						new FakeLine(contactPointsA[jt].Point, contactPointsA[jtSecondP].Point),
						out float3 projectPoint);

					if (notParallel)
					{
						float vct1 = math.lengthsq(contactPointsB[it].Point - projectPoint);
						float vct2 = math.lengthsq(contactPointsB[itSecondP].Point - projectPoint);

						float lengthAxisB = math.lengthsq(contactPointsB[itSecondP].Point - contactPointsB[it].Point);
						float lengthAxisA = math.lengthsq(contactPointsA[jtSecondP].Point - contactPointsA[jt].Point);

						if (vct1 > lengthAxisA || vct2 > lengthAxisA)
						{
							continue;
						}

						if (vct1 > lengthAxisB || vct2 > lengthAxisB)
						{
							continue;
						}

						contactPointsBuffer.Add(projectPoint);
					}
				}
			}

			for (int it = 0; it < contactPointsA.Count; it++)
			{
				if (CollisionComputations.IsInside(contactPointsB, contactPointsA[it].Point, normal))
				{
					contactPointsBuffer.Add(contactPointsA[it].Point);
				}
			}
		}
	}

	public static partial class CollisionComputations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryCalculatePenetration(
			FakeBoxCollider collider,
			FakePose pose,
			FakeBoxCollider otherCollider,
			FakePose otherPose,
			out FakeContactPair contactPair)
		{
			var hasPenetration = Physics.ComputePenetration(
				collider.UnityBoxCollider,
				pose.Position,
				pose.Rotation,
				otherCollider.UnityBoxCollider,
				otherPose.Position,
				otherPose.Rotation,
				out var direction,
				out var distance);

			if (hasPenetration)
			{
				contactPair = new FakeContactPair(-direction, distance);
				return true;
			}

			contactPair = default;
			return false;
		}
	}
}
