Alliance Calendar control provides below features :
 
* Full month Calendar view
* Navigation forward and backward – Monthly Views
* Multi language support for week names(based on device language settings)
* Passing Custom week names 
* Highlight weekends
* Showing current month only
* Handling events on month change, selecting date etc.
* Passing list of Highlight dates (Data binding)
* Navigation to a particular month in an year
* and more …


**Accessing control inside the code**

```
using com.alliance.calendar;

namespace FullCalendar
{
    public class CalendarLayout : Activity
    {
	    CustomCalendar CalendarControl;
    	protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.samplecal);
			CalendarControl = FindViewById<CustomCalendar>(Resource.Id.CalendarControl);
		}
	}
}

```

**Set Navigation buttons text**


```
 CalendarControl.NextButtonText= "Next";
 CalendarControl.PreviousButtonText= "Prev";
```
**Show, hide Navigation Buttons**

```
CalendarControl.NextButtonVisibility= ViewStates.Invisible;
```

**Styling Navigation Buttons**

```
CalendarControl.PreviousButtonStyleId = Resource.Drawable.default_dim_selector;
```

**Hide Navigation**

```
CalendarControl.ShowOnlyCurrentMonth = true;
```

**Show Calendar from custom month**

```
CalendarControl.ShowFromDate = new DateTime();
```

**Set Button Styles**

```
CalendarControl.CalendarDaysStyleId = Resource.Style.calendar_default;
CalendarControl.WeekendDaysStyleId = Resource.Style.calendar_default_weekend;
CalendarControl.WeekendDaysBackgroundStyleId = Resource.Drawable.default_dim_selector;
```

**Set Calendar Cell Styles**

```
CalendarControl.CalendarDaysStyleId = Resource.Style.calendar_default;
CalendarControl.WeekendDaysStyleId = Resource.Style.calendar_default_weekend;
CalendarControl.WeekendDaysBackgroundStyleId = Resource.Drawable.default_dim_selector;
```

**Calendar Events**

```
CalendarControl.OnCalendarMonthChange += CalendarControl_CalendarMonthChange;
CalendarControl.OnCalendarSelectedDate += CalendarControl_CalendarDateSelected;


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

```

**Binding customdata to highlight dates**

```
private List<CustomCalendarData> customData = new List<CustomCalendarData>();

customData = new List<CustomCalendarData>
				{
                	new CustomCalendarData(DateTime.Now.AddDays(2)),
                	new CustomCalendarData(DateTime.Now.AddDays(4)),
                	new CustomCalendarData(DateTime.Now.AddDays(-4))
                };
CalendarControl.CustomDataAdapter = customData;
```

**Handling exception**

```
CalendarControl.OnCalendarException += new CalendarExceptionEventHandler(CalendarControl_OnCalendarException);

void CalendarControl_OnCalendarException(object sender, CalendarExceptionEventArgs e)
{
	// Exception Handling
}
```