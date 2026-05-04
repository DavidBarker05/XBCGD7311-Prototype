using System.Runtime.CompilerServices;

namespace Util
{
	namespace ObjectUtils
	{
		public static class Objects
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsValid<T>(T t) => t is object o && o != null;
		}
	}
}
