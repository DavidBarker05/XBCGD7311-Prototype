using System;
using System.Runtime.CompilerServices;
using Util.ArrayUtils;

namespace Util
{
	namespace ComparisonUtils
	{
		public static class Compare
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool CompareTo<T>(this T first, Func<T, T, bool> compare, T second, T third, params T[] rest)
			{
				if (!compare(first, second)) return false;
				if (!compare(second, third)) return false;
				if (!Arrays.IsValid(rest)) return true; // compare(first, second) is true and compare(second, third) is true, but no more to compare therefore true
				if (!compare(third, rest[0])) return false;
				for (int i = 0; i < rest.Length - 1; ++i)
				{
					if (!compare(rest[i], rest[i + 1])) return false;
				}
				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool Equals(this object first, object second, object third, params object[] rest) => first.CompareTo((object a, object b) => a.Equals(b), second, third, rest);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool Equals<T>(this T first, T second, T third, params T[] rest) where T : IComparable<T> => first.CompareTo((T a, T b) => a.CompareTo(b) == 0, second, third, rest);
		}
	}
}
