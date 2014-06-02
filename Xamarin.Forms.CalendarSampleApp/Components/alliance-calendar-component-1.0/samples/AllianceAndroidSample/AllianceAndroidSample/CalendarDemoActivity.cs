using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using com.alliance.calendar;
using System.Collections.Generic;

namespace AllianceAndroidSample
{
	[Activity (Label = "Alliance Calendar Demo", MainLauncher = true)]
	public class CalendarDemoActivity : Activity
	{
		CustomCalendar CalendarControl;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			CalendarControl = FindViewById<CustomCalendar>(Resource.Id.CalendarControl);
			CalendarControl.NextButtonText= "Next";
			CalendarControl.PreviousButtonText= "Prev";

			//CalendarControl.NextButtonVisibility= ViewStates.Invisible;
			//CalendarControl.PreviousButtonStyleId = Resource.Drawable.default_dim_selector;

			//CalendarControl.ShowOnlyCurrentMonth = true;
			CalendarControl.ShowFromDate = new DateTime();


			List<CustomCalendarData> customData = new List<CustomCalendarData>();

			customData = new List<CustomCalendarData>
			{
				new CustomCalendarData(DateTime.Now.AddDays(2)),
				new CustomCalendarData(DateTime.Now.AddDays(4)),
				new CustomCalendarData(DateTime.Now.AddDays(-4))
			};
			CalendarControl.CustomDataAdapter = customData;


			CalendarControl.OnCalendarMonthChange += CalendarControl_CalendarMonthChange;
			CalendarControl.OnCalendarSelectedDate += CalendarControl_CalendarDateSelected;

		}

		private void CalendarControl_CalendarDateSelected(object sender, CalendarDateSelectionEventArgs e)
		{
			Toast.MakeText(this, e.SelectedDate.ToString(), ToastLength.Short).Show();
		}

		private void CalendarControl_CalendarMonthChange(object sender, CalendarNavigationEventArgs e)
		{
			if (e.MonthChange == CalendarHelper.MonthChangeOn.Next)
			{

			}
			else if (e.MonthChange == CalendarHelper.MonthChangeOn.Previous)
			{

			}
		}
	}
}


