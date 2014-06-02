using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Calendar;
using Xamarin.Forms.Calendar.Platform.WP;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(CalendarView), typeof(CalendarViewRenderer))]

namespace Xamarin.Forms.Calendar.Platform.WP
{
    public class CalendarViewRenderer : ViewRenderer<Xamarin.Forms.Calendar.CalendarView, WPControls.Calendar>
    {
        protected override void OnModelSet()
        {
            base.OnModelSet();
            var calendar = new WPControls.Calendar();
            calendar.DateClicked +=
                (object sender, WPControls.SelectionChangedEventArgs e) =>
                {
                    Model.NotifyDateSelected(e.SelectedDate);
                };
            this.SetNativeControl(calendar);
        }
    }
}
