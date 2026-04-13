using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Util
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
        public static bool IsInRange(int value, int min, int max, RangeBounds rangeBounds = RangeBounds.ExclusiveMinExclusiveMax)
        {
            int checkMin = value - min;
            int checkMax = max - value;
            return rangeBounds switch {
                RangeBounds.ExclusiveMinExclusiveMax => checkMin > 0 && checkMax > 0,
                RangeBounds.ExclusiveMinInclusiveMax => checkMin > 0 && checkMax >= 0,
                RangeBounds.InclusiveMinExclusiveMax => checkMin >= 0 && checkMax > 0,
                RangeBounds.InclusiveMinInclusiveMax => checkMin >= 0 && checkMax >= 0,
                _ => throw new ArgumentException("Somehow you have a range bounds value that doesn't exist")
            };
        }

        public static readonly float ApproximateEpsilon = Mathf.Epsilon * 8f; // Found this in Mathf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(float value, float min, float max, bool bNearlyEqual = false, RangeBounds rangeBounds = RangeBounds.ExclusiveMinExclusiveMax) => rangeBounds switch
        {
            RangeBounds.ExclusiveMinExclusiveMax => bNearlyEqual ? (value > min - ApproximateEpsilon && value < max + ApproximateEpsilon) : (value > min && value < max),
            RangeBounds.ExclusiveMinInclusiveMax => bNearlyEqual ? (value > min - ApproximateEpsilon && value <= max + ApproximateEpsilon) : (value > min && value <= max),
            RangeBounds.InclusiveMinExclusiveMax => bNearlyEqual ? (value >= min - ApproximateEpsilon && value < max + ApproximateEpsilon) : (value >= min && value < max),
            RangeBounds.InclusiveMinInclusiveMax => bNearlyEqual ? (value >= min - ApproximateEpsilon && value <= max + ApproximateEpsilon) : (value >= min && value <= max),
            _ => throw new ArgumentException("Somehow you have a range bounds value that doesn't exist")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeExclusive(int value, int min, int max) => IsInRange(value, min, max, RangeBounds.ExclusiveMinExclusiveMax);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeExclusive(float value, float min, float max, bool bNearlyEqual = false) => IsInRange(value, min, max, bNearlyEqual, RangeBounds.ExclusiveMinExclusiveMax);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(int value, int min, int max) => IsInRange(value, min, max, RangeBounds.InclusiveMinInclusiveMax);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(float value, float min, float max, bool bNearlyEqual = false) => IsInRange(value, min, max, bNearlyEqual, RangeBounds.InclusiveMinInclusiveMax);
    }
    
    public static class Types
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType(Type type1, Type type2) => type1 == type2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T>(Type type) => CompareType(type, typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T>(Type type, T t) => CompareType(type, typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T>(T t, Type type) => CompareType(typeof(T), type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T1, T2>() => CompareType(typeof(T1), typeof(T2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T1, T2>(T1 t1) => CompareType(typeof(T1), typeof(T2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T1, T2>(T2 t2) => CompareType(typeof(T1), typeof(T2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareType<T1, T2>(T1 t1, T2 t2) => CompareType(typeof(T1), typeof(T2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidObject<T>() => typeof(T) is object o && o != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidObject<T>(T t) => typeof(T) is object o && o != null;
    }

    public static class Arrays
    {
        private static bool InternalIndexOf<T>(Array array, ref int[] indices, int currentDimension, T value)
        {
            if (array.GetLength(currentDimension) == 0)
            {
                indices[currentDimension] = -1;
                return false;
            }
            for (int i = 0; i < array.GetLength(currentDimension); ++i)
            {
                indices[currentDimension] = i;
                if (currentDimension == array.Rank - 1)
                {
                    if (array.GetValue(indices) == value as object) return true;
                }
                else if(InternalIndexOf(array, ref indices, currentDimension + 1, value)) return true;
            }
            indices[currentDimension] = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetType(Array array) => array.GetType().GetElementType();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidArray(Array array) => array != null && array.Length > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] IndexOf<T>(Array array, T value)
        {
            if (array == null) return new int[1] { -1 };
            int[] indices = new int[array.Rank];
            Array.Fill(indices, -1);
            if (!Types.CompareType<T>(GetType(array)) || !Types.IsValidObject<T>()) return indices;
            InternalIndexOf(array, ref indices, 0, value);
            return indices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(int arrayLength, int index)
        {
            if (arrayLength == 0) return false;
            return RangeCheck.IsInRange(index, 0, arrayLength, RangeCheck.RangeBounds.InclusiveMinExclusiveMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(Array array, params int[] indices)
        {
            if (!IsValidArray(array) || !IsValidArray(indices) || indices.Length > array.Rank) return false;
            bool isValid = true;
            for (int i = 0; i < indices.Length; ++i)
            {
                isValid &= IsValidIndex(array.GetLength(i), indices[i]);
                if (!isValid) break;
            }
            return isValid;
        }
    }

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
        public static void Assert(bool condition, string message = "Assert condition failed")
        {
            if (condition) return;
            Debug.LogError(message);
            Exit(1);
        }
    }

    public static class UnityUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetParentObject(GameObject go) => go.transform.parent.gameObject;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject GetGameObject(RaycastHit hitInfo) => hitInfo.collider.gameObject;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(RaycastHit hitInfo) => hitInfo.collider.gameObject.GetComponent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(Collider collider) => collider.gameObject.GetComponent<T>();
    }

    public static class Compare
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiCompare(Func<object, object, bool> compare, object first, object second, object third, params object[] rest)
        {
            if (!compare(first, second)) return false;
            if (!compare(second, third)) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first == second == third, but no more to compare therefore true
            if (!compare(third, rest[0])) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (!compare(rest[i], rest[i + 1])) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiEqual<T>(T first, T second, T third, params T[] rest) where T : IComparable<T>
        {

            if (first.CompareTo(second) != 0) return false;
            if (second.CompareTo(third) != 0) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first == second == third, but no more to compare therefore true
            if (third.CompareTo(rest[0]) != 0) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (rest[i].CompareTo(rest[i + 1]) != 0) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiEqual(object first, object second, object third, params object[] rest) => MultiCompare((object a, object b) => a.Equals(b), first, second, third, rest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiLess<T>(T first, T second, T third, params T[] rest) where T : IComparable<T>
        {
            if (first.CompareTo(second) >= 0) return false;
            if (second.CompareTo(third) >= 0) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first < second < third, but no more to compare therefore true
            if (third.CompareTo(rest[0]) >= 0) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (rest[i].CompareTo(rest[i + 1]) >= 0) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiLessOrEqual<T>(T first, T second, T third, params T[] rest) where T : IComparable<T>
        {
            if (first.CompareTo(second) > 0) return false;
            if (second.CompareTo(third) > 0) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first <= second <= third, but no more to compare therefore true
            if (third.CompareTo(rest[0]) > 0) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (rest[i].CompareTo(rest[i + 1]) > 0) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiGreater<T>(T first, T second, T third, params T[] rest) where T : IComparable<T>
        {
            if (first.CompareTo(second) <= 0) return false;
            if (second.CompareTo(third) <= 0) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first > second > third, but no more to compare therefore true
            if (third.CompareTo(rest[0]) <= 0) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (rest[i].CompareTo(rest[i + 1]) <= 0) return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MultiGreaterOrEqual<T>(T first, T second, T third, params T[] rest) where T : IComparable<T>
        {
            if (first.CompareTo(second) < 0) return false;
            if (second.CompareTo(third) < 0) return false;
            if (!Arrays.IsValidArray(rest)) return true; // first >= second >= third, but no more to compare therefore true
            if (third.CompareTo(rest[0]) < 0) return false;
            for (int i = 0; i < rest.Length - 1; ++i)
            {
                if (rest[i].CompareTo(rest[i + 1]) < 0) return false;
            }
            return true;
        }
    }
}
