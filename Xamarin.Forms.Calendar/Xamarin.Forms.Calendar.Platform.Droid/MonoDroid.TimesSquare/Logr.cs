using System;
using Android.Util;

namespace Xamarin.Forms.Calendar.Platform.Droid
{
	public class Logr
	{
		public static void D(string message)
		{
#if DEBUG
			Log.Debug("Xamarin.Forms.Calendar.Platform.Droid", message);
#endif
		}

		public static void D(string message, params object[] args)
		{
#if DEBUG
			D(string.Format(message, args));
#endif
		}
	}
}