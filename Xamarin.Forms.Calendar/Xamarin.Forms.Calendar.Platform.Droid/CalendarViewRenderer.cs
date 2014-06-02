using System;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Xamarin.Forms.Calendar;
using Android.Widget;
using Android.Views;
using Android.App;
using System.Collections.Generic;
using Xamarin.Forms.Calendar.Platform.Droid;

[assembly: ExportRenderer (typeof (Xamarin.Forms.Calendar.CalendarView), typeof (CalendarViewRenderer))]

namespace Xamarin.Forms.Calendar.Platform.Droid
{
	public class CalendarViewRenderer : NativeRenderer 
	{
		private const string TAG = "Xamarin.Forms.Calendar";

		CalendarView _view;
		CalendarPickerView _pickerView;

		public CalendarViewRenderer ()
		{
		}

		protected override void OnModelChanged (VisualElement oldModel, VisualElement newModel)
		{
			_view = (CalendarView)newModel;

			base.OnModelChanged (oldModel, newModel);

			LayoutInflater inflatorservice = (LayoutInflater) Context.GetSystemService(Android.Content.Context.LayoutInflaterService);
			var containerView = (LinearLayout)inflatorservice.Inflate (Resource.Layout.calendar_picker, null, false);

			_pickerView = containerView.FindViewById<CalendarPickerView>(Resource.Id.calendar_view);
			_pickerView.Init (new DateTime (2014, 6, 1), new DateTime (2014, 6, 30))
				.InMode (CalendarPickerView.SelectionMode.Single);

			_pickerView.OnDateSelected += (s, e) => {
				_view.NotifyDateSelected(e.SelectedDate);
			};

			SetNativeControl (containerView);
		}
	}
}
