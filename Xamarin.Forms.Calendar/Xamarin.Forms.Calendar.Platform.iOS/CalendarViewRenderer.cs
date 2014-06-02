using System;
using Xamarin.Forms.Platform.iOS;
using MonoTouch.UIKit;
using Xamarin.Forms;
using escoz;
using Xamarin.Forms.Calendar;

[assembly: ExportRenderer (typeof (CalendarView), typeof (CalendarViewRenderer))]

namespace Xamarin.Forms.Calendar
{
	public class CalendarViewRenderer : NativeRenderer 
	{
		CalendarView _view;
		public CalendarViewRenderer ()
		{
		}

		protected override void OnModelSet (VisualElement model)
		{
			_view = (CalendarView)model;
			base.OnModelSet (model);

			var calendarView = new CalendarMonthView(DateTime.Now, true);

			calendarView.OnDateSelected += (date) => {
				_view.NotifyDateSelected(date);
			};

			base.SetNativeControl (calendarView);
		}			
	}
}

