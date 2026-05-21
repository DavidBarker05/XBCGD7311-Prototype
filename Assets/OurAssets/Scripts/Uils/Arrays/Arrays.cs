using System;
using System.Runtime.CompilerServices;
using Util.RangeCheckUtils;

namespace Util
{
	namespace ArrayUtils
	{
		public static partial class Arrays
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsValid(Array array) => array != null && array.Length > 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsJagged(this Array array) => array.GetType().GetElementType().IsArray;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsMultidimensional(this Array array) => array.Rank > 1;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsSingleDimensional(this Array array) => !array.IsJagged() && !array.IsMultidimensional();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Type GetStoredType(this Array array) => IsJagged(array) ? ArraysInternal.GetTypeJagged(array) : array.GetType().GetElementType();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsValidIndex(int arrayLength, int index) => index.IsInRange(0, arrayLength, RangeCheck.RangeBounds.InclusiveMinExclusiveMax);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool ContainsIndex(this Array array, int index) => !array.IsMultidimensional() && IsValidIndex(array.Length, index);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool ContainsIndex(this Array array, int firstIndex, int secondIndex, params int[] remainingIndices)
			{
				if (array.IsJagged()) return ArraysInternal.IsValidIndexJagged(array, firstIndex, secondIndex, remainingIndices);
				if (array.IsMultidimensional()) return ArraysInternal.IsValidIndexMultidimensional(array, firstIndex, secondIndex, remainingIndices);
				return false; // One dimensional array
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool ContainsIndex(this Array array, int[] indices)
			{
				if (!IsValid(indices)) return false;
				int firstIndex = indices[0];
				if (indices.Length > 1)
				{
					int secondIndex = indices[1];
					if (indices.Length > 2)
					{
						int[] remainingIndices = indices.SubArray(2);
						return array.ContainsIndex(firstIndex, secondIndex, remainingIndices);
					}
					return array.ContainsIndex(firstIndex, secondIndex);
				}
				return array.ContainsIndex(firstIndex);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static object GetValueJagged(this Array array, int[] index) => throw new NotImplementedException("Jagged array implementation hasn't been added");

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void SetValueJagged(this Array array, object value) => throw new NotImplementedException("Jagged array implementation hasn't been added");

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetValue<T>(this Array array, int index)
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.GetValue<T>(int index) can't be used with multidimensional arrays");
				Type arrayType = array.IsJagged() ? array.GetType().GetElementType() : array.GetStoredType();
				if (arrayType != typeof(T)) throw new ArgumentException("Return type does not match the type stored in the array");
				return (T)array.GetValue(index);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetValue<T>(this Array array, int[] index)
			{
				if (!IsValid(index)) throw new ArgumentException("Array.GetValue<T>(int[] index) can't be used with multidimensional arrays");
				if (array.GetType().GetElementType() != typeof(T)) throw new ArgumentException("Return type does not match the type stored in the array");
				if (array.IsMultidimensional()) return (T)array.GetValue(index);
				if (array.IsJagged()) return (T)array.GetValueJagged(index);
				return (T)array.GetValue(index[0]);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Swap(this Array array, int firstIndex, int secondIndex)
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.Swap(int firstIndex, int secondIndex) can't be used with multidimensional arrays");
				object o = array.GetValue(firstIndex);
				array.SetValue(array.GetValue(secondIndex), firstIndex);
				array.SetValue(o, secondIndex);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Swap(this Array array, int[] firstIndex, int[] secondIndex)
			{
				if (!IsValid(firstIndex)) throw new ArgumentException("First index is an invalid array");
				if (!IsValid(secondIndex)) throw new ArgumentException("Second index is an invalid array");
				if (array.IsJagged()) ArraysInternal.SwapJagged(array, firstIndex, secondIndex);
				else if (array.IsMultidimensional()) ArraysInternal.SwapMultidimensional(array, firstIndex, secondIndex);
				else array.Swap(firstIndex[0], secondIndex[0]);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int[] MultiIndexOf<T>(this Array array, T value)
			{
				if (array.IsJagged()) return ArraysInternal.IndexOfJagged(array, value);
				if (array.IsMultidimensional()) return ArraysInternal.IndexOfMultidimensional(array, value);
				return new int[1] { ArraysInternal.IndexOfSingleDimensional(array, value) };
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Shuffle(this Array array)
			{
				if (array.IsJagged()) ArraysInternal.ShuffleJagged(array);
				else if (array.IsMultidimensional()) ArraysInternal.ShuffleMultidimensional(array);
				else ArraysInternal.ShuffleSingleDimensional(array);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int GetRandomIndex(this Array array)
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.GetRandomIndex() can't be used with multidimensional arrays");
				return ArraysInternal.GetRandomIndexSingleDimensional(array);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int[] GetRandomMultiIndex(this Array array)
			{
				if (array.IsJagged()) return ArraysInternal.GetRandomIndexJagged(array);
				if (array.IsMultidimensional()) return ArraysInternal.GetRandomIndexMultidimensional(array);
				return new int[1] { ArraysInternal.GetRandomIndexSingleDimensional(array) };
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static object GetRandomElement(this Array array)
			{
				int[] randomIndex = array.GetRandomMultiIndex();
				if (array.IsJagged()) return array.GetValueJagged(randomIndex);
				if (array.IsMultidimensional()) return array.GetValue(randomIndex);
				return array.GetValue(randomIndex[0]);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetRandomElement<T>(this Array array) => array.GetValue<T>(array.GetRandomMultiIndex());

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T[] SubArray<T>(this T[] array, int startIndex, int length)
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.SubArray(int startIndex, int length) can't be used with multidimensional arrays");
				T[] subArray = new T[length];
				Array.Copy(array, startIndex, subArray, 0, length);
				return subArray;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T[] SubArray<T>(this T[] array, int startIndex) => array.SubArray(startIndex, array.Length - startIndex);
		}
	}
}
