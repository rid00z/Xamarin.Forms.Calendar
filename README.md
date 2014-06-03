
### What the app looks like on each platform(these are real native UI components):

![Alt text](http://www.michaelridland.com/wp-content/uploads/2014/06/platform-image.png) 

### The common code that used for the UI:

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

### The (Android)platform code:
```cs
Xamarin.Forms.Forms.Init (this, bundle);
SetPage (App.GetMainPage ());
```
### The (iOS)platform code:
```cs
window.RootViewController = App.GetMainPage ().CreateViewController ();
```

### The (Windows Phone)platform code

```cs
Content = Xamarin.Forms.CalendarSampleApp.App.GetMainPage().ConvertPageToUIElement(this);
```

Now this is a trivial app, but as you can imagine as the application becomes more complex your UI code will stay in your common library and work across all platforms.

### Please see this blog post for more info: 

http://www.michaelridland.com/xamarin/xamarin-forms-contest/
