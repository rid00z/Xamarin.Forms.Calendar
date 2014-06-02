using System;
using Xamarin.Forms;
using Xamarin.Forms.Calendar;

namespace Xamarin.Forms.CalendarSampleApp
{
	public class App
	{
		public static Page GetMainPage ()
		{	
			return new NavigationPage(
				new SampleCalendarPage()
			);
		}
	}
}

