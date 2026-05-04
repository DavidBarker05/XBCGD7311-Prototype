using System.Runtime.CompilerServices;
using UnityEngine;

namespace Util
{
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
			public static T AssertType<T>(object value, string argumentName = "")
			{
				if (value is T t) return t;
				argumentName = argumentName.Trim();
				string message = argumentName == "" ? "Type assertion failed" : $"Type of {argumentName} does not match the type {typeof(T).FullName}";
				Debug.LogAssertion(message);
				Exit(1);
				return default;
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
}
