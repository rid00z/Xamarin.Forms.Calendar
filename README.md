
### What the app looks like on each platform(these are real native UI components):

![Alt text](http://www.michaelridland.com/wp-content/uploads/2014/06/platform-image.png) 

### Please see this blog post for more info: 

http://www.michaelridland.com/xamarin/xamarin-forms-contest/

```cs
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
		_calendarView.DateSelected += (object sender, DateTime e) =&gt; {
			_stacker.Children.Add(new Label() 
				{ 
					Text = "Date Was Selected" + e.ToString("d"),
					VerticalOptions = LayoutOptions.Start,
					HorizontalOptions = LayoutOptions.CenterAndExpand,
				});
		};

	}
}
```
