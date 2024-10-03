using System;
using Unity.Mathematics;

namespace FakePhysics.SoftBodyDynamics
{
	public class RopeArgs
	{
		private readonly float m_SpanDistance = 1f;
		private readonly float m_Drag = 0f;
		private readonly float m_Density = 7800f;
		private readonly float m_Radius = 0.1f;
		private readonly float m_Stiffness = 0f;
		private readonly float m_Stretch = 0.5f;
		private readonly bool m_NeedDistanceConstraint = true;
		private readonly bool m_NeedBendConstraint = true;
		private readonly bool m_UseStretchFactor = false;

		public RopeArgs(RopeArgsBuilder builder)
		{
			m_SpanDistance = builder.SpanDistance;
			m_Drag = builder.Drag;
			m_Density = builder.Density;
			m_Radius = builder.Radius;
			m_Stiffness = builder.Stiffness;
			m_Stretch = builder.Stretch;
			m_NeedDistanceConstraint = builder.NeedDistanceConstraint;
			m_NeedBendConstraint = builder.NeedBendConstraint;
			m_UseStretchFactor = builder.UseStretchFactor;
		}

		public float SpanDistance => m_SpanDistance;

		public float Drag => m_Drag;

		public float Density => m_Density;

		public float Radius => m_Radius;

		public float Stiffness => m_Stiffness;

		public float Stretch => m_Stretch;

		public bool NeedDistanceConstraint => m_NeedDistanceConstraint;

		public bool NeedBendConstraint => m_NeedBendConstraint;

		public bool UseStretchFactor => m_UseStretchFactor;

		public float Mass => m_Density * Volume;

		public float Volume => math.PI * m_Radius * m_Radius * m_SpanDistance;
	}

	public class RopeArgsBuilder
	{
		public float SpanDistance = 1f;
		public float Drag = 0f;
		public float Density = 7800f;
		public float Radius = 0.1f;
		public float Stiffness = 0f;
		public float Stretch = 0.5f;
		public bool NeedDistanceConstraint = true;
		public bool NeedBendConstraint = true;
		public bool UseStretchFactor = false;

		public RopeArgsBuilder SetSpanDistance(float spanDistance)
		{
			SpanDistance = spanDistance;
			return this;
		}

		public RopeArgsBuilder SetDrag(float drag)
		{
			Drag = drag;
			return this;
		}

		public RopeArgsBuilder SetDensity(float density)
		{
			Density = density;
			return this;
		}

		public RopeArgsBuilder SetRadius(float radius)
		{
			Radius = radius;
			return this;
		}

		public RopeArgsBuilder SetStiffness(float stiffness)
		{
			Stiffness = stiffness;
			return this;
		}

		public RopeArgsBuilder SetStretch(float stretch)
		{
			Stretch = stretch;
			return this;
		}

		public RopeArgsBuilder SetNeedDistanceConstraint(bool needDistanceConstraint)
		{
			NeedDistanceConstraint = needDistanceConstraint;
			return this;
		}

		public RopeArgsBuilder SetNeedBendConstraint(bool needBendConstraint)
		{
			NeedBendConstraint = needBendConstraint;
			return this;
		}

		public RopeArgs SetUseStretchFactor(bool useStretchFactor)
		{
			UseStretchFactor = useStretchFactor;
			return this;
		}

		public RopeArgs Build()
		{
			return new RopeArgs(this);
		}

		public static implicit operator RopeArgs(RopeArgsBuilder build)
		{
			return new RopeArgs(build);
		}
	}
}
