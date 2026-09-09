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
				public static bool IsValidIndexJagged(Array array, int firstIndex, int secondIndex, params int[] remainingIndices)
				{
					Array current = array;
					if (firstIndex < 0 || firstIndex >= current.Length) return false;
					if (current.GetValue(firstIndex) is not Array first) return false;
					current = first;
					if (secondIndex < 0 || secondIndex >= current.Length) return false;
					if (remainingIndices == null || remainingIndices.Length == 0) return true; // No more indices to check so valid and valid so far so valid (jagged array accessing an array is still a valid index so even if technically more nested arrays the index is valid)
					if (current.GetValue(secondIndex) is not Array second) return false; // More indices to check but no more nested arrays so invalid
					current = second;
					int currentDepth = 0;
					while (true)
					{
						int index = remainingIndices[currentDepth];
						if (index < 0 || index >= current.Length) return false;
						if (currentDepth == remainingIndices.Length - 1) return true; // No more indices to check so valid and valid so far so valid (jagged array accessing an array is still a valid index so even if technically more nested arrays the index is valid)
						if (current.GetValue(index) is not Array next) return false;
						current = next;
						++currentDepth;
					}
				}

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
						if (EqualityComparer<T>.Default.Equals((T)array.GetValue(i), value)) return i;
					}
					return -1;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] IndexOfJagged<T>(Array array, T value)
				{
					if (GetTypeJagged(array) is not T || !Objects.IsValid(value)) return new int[] { -1 };
					Stack<(Array, List<int>)> stack = new Stack<(Array, List<int>)>();
					stack.Push((array, new List<int>()));
					while (stack.Count > 0)
					{
						var (current, currentIndices) = stack.Pop();
						for (int i = 0; i < current.Length; ++i)
						{
							object item = current.GetValue(i);
							List<int> newIndices = new(currentIndices);
							newIndices.Add(i);
							if (item is Array nested) stack.Push((nested, newIndices));
							else if (EqualityComparer<T>.Default.Equals((T)item, value)) return newIndices.ToArray();
						}
					}
					return new int[] { -1 };
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] IndexOfMultidimensional<T>(Array array, T value)
				{
					int[] indices = new int[array.Rank];
					if (array.GetStoredType() is not T || !Objects.IsValid(value))
					{
						Array.Fill(indices, -1);
						return indices;
					}
					for (int i = 0; i < array.Length; ++i)
					{
						int remainder = i;
						for (int j = array.Rank - 1; j >= 0; --j)
						{
							int length = array.GetLength(j);
							indices[j] = remainder % length;
							remainder /= length;
						}
						if (EqualityComparer<T>.Default.Equals((T)array.GetValue(indices), value)) return (int[])indices.Clone();
					}
					Array.Fill(indices, -1);
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
				public static void ShuffleJagged(Array array)
				{
					Stack<Array> stack = new Stack<Array>();
					stack.Push(array);
					while (stack.Count > 0)
					{
						Array current = stack.Pop();
						for (int i = 0; i < current.Length; ++i)
						{
							if (current.GetValue(i) is Array nested) stack.Push(nested);
						}
						ShuffleSingleDimensional(current);
					}
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static void ShuffleMultidimensional(Array array) => throw new NotImplementedException("Multidimensional array implementation hasn't been added");

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int GetRandomIndexSingleDimensional(Array array) => UnityEngine.Random.Range(0, array.Length);

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] GetRandomIndexJagged(Array array)
				{
					List<int> index = new List<int>();
					Array arrayIteration = array;
					while (arrayIteration != null)
					{
						int randomIndex = arrayIteration.GetRandomIndex();
						index.Add(randomIndex);
						arrayIteration = arrayIteration.IsJagged() ? (Array)arrayIteration.GetValue(randomIndex) : null;
					}
					if (index.Count == 0) index.Add(-1);
					return index.ToArray();
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static int[] GetRandomIndexMultidimensional(Array array)
				{
					int[] index = new int[array.Rank];
					for (int i = 0; i < index.Length; ++i)
					{
						int dimensionLength = array.GetLength(i);
						index[i] = UnityEngine.Random.Range(0, dimensionLength);
					}
					return index;
				}
			}
		}
	}
}
