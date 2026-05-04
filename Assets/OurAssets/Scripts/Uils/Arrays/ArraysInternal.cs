using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Util.ObjectUtils;

namespace Util
{
	namespace ArrayUtils
	{
		public static partial class Arrays
		{
			private static class ArraysInternal
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Type GetTypeJagged(Array array)
				{
					Type type = array.GetType().GetElementType();
					do type = type.GetElementType(); while (type.IsArray);
					return type;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static bool IsValidIndexJagged(Array array, int firstIndex, int secondIndex, params int[] remainingIndices) => throw new NotImplementedException("Jagged array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static bool IsValidIndexMultidimensional(Array array, int firstIndex, int secondIndex, params int[] remainingIndices)
				{
					if (!IsValidIndex(array.GetLength(0), firstIndex) || !IsValidIndex(array.GetLength(0), secondIndex)) return false;
					if (!IsValid(remainingIndices)) return true; // First index and second index are valid, but there are no more indices to check therefore valid
					if (remainingIndices.Length > array.Rank - 2) return false; // Already checked the first 2 ranks so use array.Rank - 2
					for (int i = 0; i < remainingIndices.Length; ++i)
					{
						if (!IsValidIndex(array.GetLength(i + 2), remainingIndices[i])) return false;
					}
					return true;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void SwapJagged(Array array, int[] firstIndex, int[] secondIndex) => throw new NotImplementedException("Jagged array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void SwapMultidimensional(Array array, int[] firstIndex, int[] secondIndex) => throw new NotImplementedException("Multidimensional array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int IndexOfSingleDimensional<T>(Array array, T value)
				{
					if (array.GetStoredType() is not T || !Objects.IsValid(value)) return -1;
					for (int i = 0; i < array.Length; ++i)
					{
						if (array.GetValue(i) == value as object) return i;
					}
					return -1;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] IndexOfJagged<T>(Array array, T value) => throw new NotImplementedException("Jagged array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private static bool IndexOfMultidimensionalRecursion<T>(Array array, ref int[] indices, int currentDimension, T value)
				{
					for (int i = 0; i < array.GetLength(currentDimension); ++i)
					{
						indices[currentDimension] = i;
						if (currentDimension == array.Rank - 1)
						{
							if (array.GetValue(indices) == value as object) return true;
						}
						else if (IndexOfMultidimensionalRecursion(array, ref indices, currentDimension + 1, value)) return true;
					}
					indices[currentDimension] = -1;
					return false;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] IndexOfMultidimensional<T>(Array array, T value)
				{
					int[] indices = new int[array.Rank];
					Array.Fill(indices, -1);
					if (array.GetStoredType() is not T || !Objects.IsValid(value)) return indices;
					IndexOfMultidimensionalRecursion(array, ref indices, 0, value);
					return indices;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void ShuffleSingleDimensional(Array array)
				{
					// Fisher-Yates shuffle: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
					System.Random rng = new System.Random();
					for (int i = array.Length - 1; i > 0; --i)
					{
						int j = rng.Next(i + 1);
						array.Swap(i, j);
					}
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void ShuffleJagged(Array array) => throw new NotImplementedException("Jagged array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void ShuffleMultidimensional(Array array) => throw new NotImplementedException("Multidimensional array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int GetRandomIndexSingleDimensional(Array array) => UnityEngine.Random.Range(0, array.Length);

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] GetRandomIndexJagged(Array array)
				{
					List<int> index = new List<int>();
					index.Add(array.GetRandomIndex());
					object[] arrayIteration = (object[])array;
					while (arrayIteration != null)
					{
						int randomIndex = arrayIteration.GetRandomIndex();
						index.Add(randomIndex);
						arrayIteration = arrayIteration.GetType().GetElementType().IsArray ? (object[])arrayIteration.GetValue(randomIndex) : null;
					}
					return index.ToArray();
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] GetRandomIndexMultidimensional(Array array)
				{
					int[] index = new int[array.Rank];
					for (int i = 0; i < index.Length; ++i)
					{
						int dimensionLength = array.GetLength(i);
						if (dimensionLength == 0) throw new ArgumentException("Array contains empty dimensions");
						index[i] = UnityEngine.Random.Range(0, dimensionLength);
					}
					return index;
				}
			}
		}
	}
}
