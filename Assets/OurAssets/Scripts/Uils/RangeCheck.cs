using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Util
{
	namespace RangeCheckUtils
	{
		public static class RangeCheck
		{
			public enum RangeBounds
			{
				ExclusiveMinExclusiveMax,
				ExclusiveMinInclusiveMax,
				InclusiveMinExclusiveMax,
				InclusiveMinInclusiveMax
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRange(this int value, int min, int max, RangeBounds rangeBounds = RangeBounds.ExclusiveMinExclusiveMax) => rangeBounds switch
			{
				RangeBounds.ExclusiveMinExclusiveMax => value > min && value < max,
				RangeBounds.ExclusiveMinInclusiveMax => value > min && value <= max,
				RangeBounds.InclusiveMinExclusiveMax => value >= min && value < max,
				RangeBounds.InclusiveMinInclusiveMax => value >= min && value <= max,
				_ => throw new ArgumentException("Somehow you have a range bounds value that doesn't exist")
			};

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRange(this uint value, uint min, uint max, RangeBounds rangeBounds = RangeBounds.ExclusiveMinExclusiveMax) => rangeBounds switch
			{
				RangeBounds.ExclusiveMinExclusiveMax => value > min && value < max,
				RangeBounds.ExclusiveMinInclusiveMax => value > min && value <= max,
				RangeBounds.InclusiveMinExclusiveMax => value >= min && value < max,
				RangeBounds.InclusiveMinInclusiveMax => value >= min && value <= max,
				_ => throw new ArgumentException("Somehow you have a range bounds value that doesn't exist")
			};

			public static readonly float ApproximateEpsilon = Mathf.Epsilon * 8f; // Found this in Mathf

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRange(this float value, float min, float max, bool bNearlyEqual = false, RangeBounds rangeBounds = RangeBounds.ExclusiveMinExclusiveMax) => rangeBounds switch
			{
				RangeBounds.ExclusiveMinExclusiveMax => bNearlyEqual ? (value > min - ApproximateEpsilon && value < max + ApproximateEpsilon) : (value > min && value < max),
				RangeBounds.ExclusiveMinInclusiveMax => bNearlyEqual ? (value > min - ApproximateEpsilon && value <= max + ApproximateEpsilon) : (value > min && value <= max),
				RangeBounds.InclusiveMinExclusiveMax => bNearlyEqual ? (value >= min - ApproximateEpsilon && value < max + ApproximateEpsilon) : (value >= min && value < max),
				RangeBounds.InclusiveMinInclusiveMax => bNearlyEqual ? (value >= min - ApproximateEpsilon && value <= max + ApproximateEpsilon) : (value >= min && value <= max),
				_ => throw new ArgumentException("Somehow you have a range bounds value that doesn't exist")
			};

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeExclusive(this int value, int min, int max) => value.IsInRange(min, max, RangeBounds.ExclusiveMinExclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeExclusive(this uint value, uint min, uint max) => value.IsInRange(min, max, RangeBounds.ExclusiveMinExclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeExclusive(this float value, float min, float max, bool bNearlyEqual = false) => value.IsInRange(min, max, bNearlyEqual, RangeBounds.ExclusiveMinExclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeInclusive(this int value, int min, int max) => value.IsInRange(min, max, RangeBounds.InclusiveMinInclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeInclusive(this uint value, uint min, uint max) => value.IsInRange(min, max, RangeBounds.InclusiveMinInclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsInRangeInclusive(this float value, float min, float max, bool bNearlyEqual = false) => value.IsInRange(min, max, bNearlyEqual, RangeBounds.InclusiveMinInclusiveMax);
		}
	}
}
