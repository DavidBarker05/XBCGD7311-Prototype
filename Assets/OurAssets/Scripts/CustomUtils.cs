using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Util.RangeCheckUtils;
using Util.ObjectUtils;
using Util.ArrayUtils;
using System.Collections.Generic;

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
    
	namespace ObjectUtils
	{
		public static class Objects
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsValid<T>(T t) => t is object o && o != null;
		}
	}

	namespace ArrayUtils
	{
		public static class Arrays
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
			public static object GetValueJagged(this Array array, int[] index) => throw new NotImplementedException("Jagged array implementation hasn't been added");

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void SetValueJagged(this Array array, object value) => throw new NotImplementedException("Jagged array implementation hasn't been added");

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetValue<T>(this Array array, int index)
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.GetValue<T>(int index) can't be used with multidimensional arrays");
				Type arrayType = array.IsJagged() ? array.GetType().GetElementType() : array.GetStoredType();
				if (arrayType != typeof(T)) throw new ArgumentException("Return type does not match the type stored in the array");
				return (T) array.GetValue(index);
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
			public static T[] SubArray<T>(this T[] array, int startIndex, int length) where T : class
			{
				if (array.IsMultidimensional()) throw new ArgumentException("Array.SubArray(int startIndex, int length) can't be used with multidimensional arrays");
				T[] subArray = new T[length];
				Array.Copy(array, startIndex, subArray, 0, length);
				return subArray;
			}

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

	namespace SystemUtils
	{
		public static class Sys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Exit(int exitCode)
			{
#if UNITY_EDITOR
				if (exitCode == 0) Debug.Log("Exited with code 0");
				else Debug.LogError($"Exited with code {exitCode}");
				UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(exitCode);
#endif
			}

			/// <summary>
			/// Tests if the condition is valid. If the condition is not valid it logs an error
			/// message and exits the game.
			/// </summary>
			/// <param name="condition">The condition to test</param>
			/// <param name="message">The error message to log</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Assert(bool condition, string message = "Assertion failed")
			{
				if (condition) return;
				Debug.LogAssertion(message);
				Exit(1);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void AssertType<T>(object value, string argumentName = "")
			{
				if (value is T) return;
				argumentName = argumentName.Trim();
				string message = argumentName == "" ? "Type assertion failed" : $"Type of {argumentName} does not match the type {typeof(T).FullName}";
				Debug.LogAssertion(message);
				Exit(1);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void AssertTypeMessage<T>(object value, string message = "Type assertion failed")
			{
				if (value is T) return;
				Debug.LogAssertion(message);
				Exit(1);
			}
		}
	}

	namespace UnityUtils
	{
		public static class UnityUtil
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static GameObject GetParentObject(this GameObject go) => go.transform.parent.gameObject;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static GameObject GetGameObject(this RaycastHit hitInfo) => hitInfo.collider.gameObject;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetComponent<T>(this RaycastHit hitInfo) => hitInfo.collider.gameObject.GetComponent<T>();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T GetComponent<T>(this Collider collider) => collider.gameObject.GetComponent<T>();
		}
	}

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
