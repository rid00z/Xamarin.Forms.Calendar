using System;
using Xamarin.Forms.Calendar;

namespace Xamarin.Forms.CalendarSampleApp
{
	public class SampleCalendarPage : ContentPage 
	{
		CalendarView _calendarView;
		StackLayout _stacker;

		public SampleCalendarPage ()
		{
			Title = "Calendar Sample";

			_stacker = new StackLayout ();
			Content = _stacker;

			_calendarView = new CalendarView() {
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			_stacker.Children.Add (_calendarView);
			_calendarView.DateSelected += (object sender, DateTime e) => {
				_stacker.Children.Add(new Label() 
					{ 
						Text = "Date Was Selected" + e.ToString("d"),
						VerticalOptions = LayoutOptions.Start,
						HorizontalOptions = LayoutOptions.CenterAndExpand,
					});
			};

		}
	}
}

