using System;

namespace Xamarin.Forms.Calendar
{
	public class CalendarView : View
	{
		public CalendarView ()
		{
		}

		public void NotifyDateSelected(DateTime dateSelected)
		{
			if (DateSelected != null)
				DateSelected (this, dateSelected);
		}

		public event EventHandler<DateTime> DateSelected;
	}
}

