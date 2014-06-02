//
//  CalendarMonthView.cs
//
//  Converted to MonoTouch on 1/22/09 - Eduardo Scoz || http://escoz.com
//  Originally reated by Devin Ross on 7/28/09  - tapku.com || http://github.com/devinross/tapkulibrary
//
/*
 
 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:
 
 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 */

using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace escoz
{
	
	public delegate void DateSelected(DateTime date);
	public delegate void MonthChanged(DateTime monthSelected);
	
	public class CalendarMonthView : UIView
	{
		public int BoxHeight = 30;
		public int BoxWidth = 46;
		int headerHeight = 0;

		public Action<DateTime> OnDateSelected;
		public Action<DateTime> OnFinishedDateSelection;
		public Func<DateTime, bool> IsDayMarkedDelegate;
		public Func<DateTime, bool> IsDateAvailable;
		public Action<DateTime> MonthChanged;
		public Action SwipedUp;

		public DateTime CurrentSelectedDate;
		public DateTime CurrentMonthYear;
		protected DateTime CurrentDate { get; set; }
		
		private UIScrollView _scrollView;
		private bool calendarIsLoaded;
		
		private MonthGridView _monthGridView;
		bool _showHeader;

		public CalendarMonthView(DateTime selectedDate, bool showHeader, float width = 320) 
		{
			_showHeader = showHeader;

			if (_showHeader)
				headerHeight = 20;

			if (_showHeader)
				this.Frame = new RectangleF(0, 0, width, 218);
			else 
				this.Frame = new RectangleF(0, 0, width, 198);
				
			BoxWidth = Convert.ToInt32(Math.Ceiling( width / 7 ));

			BackgroundColor = UIColor.White;

			ClipsToBounds = true;

			CurrentSelectedDate = selectedDate;
			CurrentDate = DateTime.Now.Date;
			CurrentMonthYear = new DateTime(CurrentSelectedDate.Year, CurrentSelectedDate.Month, 1);

			var swipeLeft = new UISwipeGestureRecognizer(p_monthViewSwipedLeft);
			swipeLeft.Direction = UISwipeGestureRecognizerDirection.Left;
			this.AddGestureRecognizer(swipeLeft);

			var swipeRight = new UISwipeGestureRecognizer(p_monthViewSwipedRight);
			swipeRight.Direction = UISwipeGestureRecognizerDirection.Right;
			this.AddGestureRecognizer(swipeRight);

			var swipeUp = new UISwipeGestureRecognizer(p_monthViewSwipedUp);
			swipeUp.Direction = UISwipeGestureRecognizerDirection.Up;
			this.AddGestureRecognizer(swipeUp);
		}

		public void SetDate (DateTime newDate)
		{
			bool right = true;

			CurrentSelectedDate = newDate;

			var monthsDiff = (newDate.Month - CurrentMonthYear.Month) + 12 * (newDate.Year - CurrentMonthYear.Year);
			if (monthsDiff != 0)
			{
				if (monthsDiff < 0)
				{
					right = false;
					monthsDiff = -monthsDiff;
				}
				
				for (int i=0; i<monthsDiff; i++)
				{
					MoveCalendarMonths (right, true);
				}
			} 
			else
			{
				RebuildGrid(right, false);
			}

		}

		void p_monthViewSwipedUp (UISwipeGestureRecognizer ges)
		{
			if (SwipedUp != null)
				SwipedUp ();
		}

		void p_monthViewSwipedRight (UISwipeGestureRecognizer ges)
		{
			MoveCalendarMonths(false, true);
		}

		void p_monthViewSwipedLeft (UISwipeGestureRecognizer ges)
		{
			MoveCalendarMonths(true, true);
		}

		public override void SetNeedsDisplay ()
		{
			base.SetNeedsDisplay();
			if (_monthGridView!=null)
				_monthGridView.Update();
		}
		
		public override void LayoutSubviews ()
		{
			if (calendarIsLoaded) return;
			
			_scrollView = new UIScrollView()
			{
				ContentSize = new SizeF(320, 260),
				ScrollEnabled = false,
				Frame = new RectangleF(0, 16 + headerHeight, 320, this.Frame.Height - 16),
				BackgroundColor = UIColor.White
			};
			
			//_shadow = new UIImageView(UIImage.FromBundle("Images/Calendar/shadow.png"));
			
			//LoadButtons();
			
			LoadInitialGrids();
			
			BackgroundColor = UIColor.Clear;

			AddSubview(_scrollView);

			//AddSubview(_shadow);

			_scrollView.AddSubview(_monthGridView);
			
			calendarIsLoaded = true;
		}
		
		public void DeselectDate(){
			if (_monthGridView!=null)
				_monthGridView.DeselectDayView();
		}
		
		/*private void LoadButtons()
		{
			_leftButton = UIButton.FromType(UIButtonType.Custom);
			_leftButton.TouchUpInside += HandlePreviousMonthTouch;
			_leftButton.SetImage(UIImage.FromBundle("Images/Calendar/leftarrow.png"), UIControlState.Normal);
			AddSubview(_leftButton);
			_leftButton.Frame = new RectangleF(10, 0, 44, 42);
			
			_rightButton = UIButton.FromType(UIButtonType.Custom);
			_rightButton.TouchUpInside += HandleNextMonthTouch;
			_rightButton.SetImage(UIImage.FromBundle("Images/Calendar/rightarrow.png"), UIControlState.Normal);
			AddSubview(_rightButton);
			_rightButton.Frame = new RectangleF(320 - 56, 0, 44, 42);
		}*/
		
		private void HandlePreviousMonthTouch(object sender, EventArgs e)
		{
			MoveCalendarMonths(false, true);
		}
		private void HandleNextMonthTouch(object sender, EventArgs e)
		{
			MoveCalendarMonths(true, true);
		}

		public void MoveCalendarMonths (bool right, bool animated)
		{
			CurrentMonthYear = CurrentMonthYear.AddMonths(right? 1 : -1);
			RebuildGrid(right, animated);
		}

		public void RebuildGrid(bool right, bool animated)
		{
			UserInteractionEnabled = false;
			
			var gridToMove = CreateNewGrid(CurrentMonthYear);
			var pointsToMove = (right? Frame.Width : -Frame.Width);
			
			/*if (left && gridToMove.weekdayOfFirst==0)
				pointsToMove += 44;
			if (!left && _monthGridView.weekdayOfFirst==0)
				pointsToMove -= 44;*/
			
			gridToMove.Frame = new RectangleF(new PointF(pointsToMove, 0), gridToMove.Frame.Size);
			
			_scrollView.AddSubview(gridToMove);
			
			if (animated){
				UIView.BeginAnimations("changeMonth");
				UIView.SetAnimationDuration(0.4);
				UIView.SetAnimationDelay(0.1);
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
			}
			
			_monthGridView.Center = new PointF(_monthGridView.Center.X - pointsToMove, _monthGridView.Center.Y);
			gridToMove.Center = new PointF(gridToMove.Center.X  - pointsToMove, gridToMove.Center.Y);
			
			_monthGridView.Alpha = 0;

			/*_scrollView.Frame = new RectangleF(
				_scrollView.Frame.Location,
				new SizeF(_scrollView.Frame.Width, this.Frame.Height-16));
			
			_scrollView.ContentSize = _scrollView.Frame.Size;*/

			SetNeedsDisplay();
			
			if (animated)
				UIView.CommitAnimations();
			
			_monthGridView = gridToMove;
			
			UserInteractionEnabled = true;

			if (MonthChanged != null)
				MonthChanged(CurrentMonthYear);
		}
		
		private MonthGridView CreateNewGrid(DateTime date){
			var grid = new MonthGridView(this, date);
			grid.CurrentDate = CurrentDate;
			grid.BuildGrid();
			grid.Frame = new RectangleF(0, 0, 320, this.Frame.Height-16);
			return grid;
		}
		
		private void LoadInitialGrids()
		{
			_monthGridView = CreateNewGrid(CurrentMonthYear);
			
			/*var rect = _scrollView.Frame;
			rect.Size = new SizeF { Height = (_monthGridView.Lines + 1) * 44, Width = rect.Size.Width };
			_scrollView.Frame = rect;*/
			
			//Frame = new RectangleF(Frame.X, Frame.Y, _scrollView.Frame.Size.Width, _scrollView.Frame.Size.Height+16);
			
			/*var imgRect = _shadow.Frame;
			imgRect.Y = rect.Size.Height - 132;
			_shadow.Frame = imgRect;*/
		}
		
		public override void Draw(RectangleF rect)
		{
			using(var context = UIGraphics.GetCurrentContext())
			{
				context.SetFillColor (UIColor.LightGray.CGColor);
				context.FillRect (new RectangleF (0, 0, 320, 18 + headerHeight));
			}

			DrawDayLabels(rect);

			if (_showHeader)
				DrawMonthLabel(rect);
		}
		
		private void DrawMonthLabel(RectangleF rect)
		{
			var r = new RectangleF(new PointF(0, 2), new SizeF {Width = 320, Height = 42});
			UIColor.DarkGray.SetColor();
			DrawString(CurrentMonthYear.ToString("MMMM yyyy"), 
				r, UIFont.BoldSystemFontOfSize(16),
			     UILineBreakMode.WordWrap, UITextAlignment.Center);
		}
		
		private void DrawDayLabels(RectangleF rect)
		{
			var font = UIFont.BoldSystemFontOfSize(10);
			UIColor.DarkGray.SetColor();
			var context = UIGraphics.GetCurrentContext();
			context.SaveState();

			var i = 0;
			foreach (var d in Enum.GetNames(typeof(DayOfWeek)))
			{
				DrawString(d.Substring(0, 3), new RectangleF(i*BoxWidth, 2 + headerHeight, BoxWidth, 10), font,
				           UILineBreakMode.WordWrap, UITextAlignment.Center);
				i++;
			}
			context.RestoreState();
		}
	}
	
	public class MonthGridView : UIView
	{
		private CalendarMonthView _calendarMonthView;
		
		public DateTime CurrentDate {get;set;}
		private DateTime _currentMonth;

		protected readonly IList<CalendarDayView> _dayTiles = new List<CalendarDayView>();
		public int Lines { get; set; }
		protected CalendarDayView SelectedDayView {get;set;}
		public int weekdayOfFirst;
		public IList<DateTime> Marks { get; set; }
		
		public MonthGridView(CalendarMonthView calendarMonthView, DateTime month)
		{
			_calendarMonthView = calendarMonthView;
			_currentMonth = month.Date;

			var tapped = new UITapGestureRecognizer(p_Tapped);
			this.AddGestureRecognizer(tapped);
		}

		void p_Tapped(UITapGestureRecognizer tapRecg)
		{
			var loc = tapRecg.LocationInView(this);
			if (SelectDayView(loc)&& _calendarMonthView.OnDateSelected!=null)
				_calendarMonthView.OnDateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag));
		}

		public void Update(){
			foreach (var v in _dayTiles)
				updateDayView(v);
			
			this.SetNeedsDisplay();
		}
		
		public void updateDayView(CalendarDayView dayView){
			dayView.Marked = _calendarMonthView.IsDayMarkedDelegate == null ? 
				false : _calendarMonthView.IsDayMarkedDelegate(dayView.Date);
			dayView.Available = _calendarMonthView.IsDateAvailable == null ? 
				true : _calendarMonthView.IsDateAvailable(dayView.Date);
		}
		
		public void BuildGrid ()
		{
			DateTime previousMonth = _currentMonth.AddMonths (-1);
			var daysInPreviousMonth = DateTime.DaysInMonth (previousMonth.Year, previousMonth.Month);
			var daysInMonth = DateTime.DaysInMonth (_currentMonth.Year, _currentMonth.Month);
			weekdayOfFirst = (int)_currentMonth.DayOfWeek;
			var lead = daysInPreviousMonth - (weekdayOfFirst - 1);

			int boxWidth = _calendarMonthView.BoxWidth;
			int boxHeight = _calendarMonthView.BoxHeight;
			
			// build last month's days
			for (int i = 1; i <= weekdayOfFirst; i++)
			{
				var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
				var dayView = new CalendarDayView (_calendarMonthView);
				dayView.Frame = new RectangleF ((i - 1) * boxWidth - 1, 0, boxWidth, boxHeight);
				dayView.Date = viewDay;
				dayView.Text = lead.ToString ();
				
				AddSubview (dayView);
				_dayTiles.Add (dayView);
				lead++;
			}
			
			var position = weekdayOfFirst + 1;
			var line = 0;
			
			// current month
			for (int i = 1; i <= daysInMonth; i++)
			{
				var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
				var dayView = new CalendarDayView(_calendarMonthView)
				{
					Frame = new RectangleF((position - 1) * boxWidth - 1, line * boxHeight, boxWidth, boxHeight),
					Today = (CurrentDate.Date==viewDay.Date),
					Text = i.ToString(),
					
					Active = true,
					Tag = i,
					Selected = (viewDay.Date == _calendarMonthView.CurrentSelectedDate.Date)
				};

				dayView.Date = viewDay;
				updateDayView (dayView);
				
				if (dayView.Selected)
					SelectedDayView = dayView;
				
				AddSubview (dayView);
				_dayTiles.Add (dayView);
				
				position++;
				if (position > 7)
				{
					position = 1;
					line++;
				}
			}
			
			//next month
			int dayCounter = 1;
			if (position != 1)
			{
				for (int i = position; i < 8; i++)
				{
					var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
					var dayView = new CalendarDayView (_calendarMonthView)
					{
						Frame = new RectangleF((i - 1) * boxWidth -1, line * boxHeight, boxWidth, boxHeight),
						Text = dayCounter.ToString(),
					};
					dayView.Date = viewDay;
					updateDayView (dayView);
					
					AddSubview (dayView);
					_dayTiles.Add (dayView);
					dayCounter++;
				}
			}

			while (line < 6)
			{
				line++;
				for (int i = 1; i < 8; i++)
				{
					var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
					var dayView = new CalendarDayView (_calendarMonthView)
					{
						Frame = new RectangleF((i - 1) * boxWidth -1, line * boxHeight, boxWidth, boxHeight),
						Text = dayCounter.ToString(),
					};
					dayView.Date = viewDay;
					updateDayView (dayView);
					
					AddSubview (dayView);
					_dayTiles.Add (dayView);
					dayCounter++;
				}
			}

			Frame = new RectangleF(Frame.Location, new SizeF(Frame.Width, (line + 1) * boxHeight));
			
			Lines = (position == 1 ? line - 1 : line);
			
			if (SelectedDayView!=null)
				this.BringSubviewToFront(SelectedDayView);
		}
		
		/*public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			if (SelectDayView((UITouch)touches.AnyObject)&& _calendarMonthView.OnDateSelected!=null)
				_calendarMonthView.OnDateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag));
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			if (SelectDayView((UITouch)touches.AnyObject)&& _calendarMonthView.OnDateSelected!=null)
				_calendarMonthView.OnDateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag));
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			if (_calendarMonthView.OnFinishedDateSelection==null) return;
			var date = new DateTime(_currentMonth.Year, _currentMonth.Month, SelectedDayView.Tag);
			if (_calendarMonthView.IsDateAvailable == null || _calendarMonthView.IsDateAvailable(date))
				_calendarMonthView.OnFinishedDateSelection(date);
		}*/
		
		private bool SelectDayView(PointF p){

			int index = ((int)p.Y / _calendarMonthView.BoxHeight) * 7 + ((int)p.X / _calendarMonthView.BoxWidth);
			if(index<0 || index >= _dayTiles.Count) return false;
			
			var newSelectedDayView = _dayTiles[index];
			if (newSelectedDayView == SelectedDayView) 
				return false;
			
			if (!newSelectedDayView.Active){
				var day = int.Parse(newSelectedDayView.Text);
				if (day > 15)
					_calendarMonthView.MoveCalendarMonths(false, true);
				else
					_calendarMonthView.MoveCalendarMonths(true, true);
				return false;
			} else if (!newSelectedDayView.Active && !newSelectedDayView.Available){
				return false;
			}
			
			if (SelectedDayView!=null)
				SelectedDayView.Selected = false;
			
			this.BringSubviewToFront(newSelectedDayView);
			newSelectedDayView.Selected = true;
			
			SelectedDayView = newSelectedDayView;
			_calendarMonthView.CurrentSelectedDate =  SelectedDayView.Date;
			SetNeedsDisplay();
			return true;
		}

		public void DeselectDayView(){
			if (SelectedDayView==null) return;
			SelectedDayView.Selected= false;
			SelectedDayView = null;
			SetNeedsDisplay();
		}
	}
	
	public class CalendarDayView : UIView
	{
		string _text;
		public DateTime Date {get;set;}
		bool _active, _today, _selected, _marked, _available;
		public bool Available {get {return _available; } set {_available = value; SetNeedsDisplay(); }}
		public string Text {get { return _text; } set { _text = value; SetNeedsDisplay(); } }
		public bool Active {get { return _active; } set { _active = value; SetNeedsDisplay();  } }
		public bool Today {get { return _today; } set { _today = value; SetNeedsDisplay(); } }
		public bool Selected {get { return _selected; } set { _selected = value; SetNeedsDisplay(); } }
		public bool Marked {get { return _marked; } set { _marked = value; SetNeedsDisplay(); }  }

		CalendarMonthView _mv;

		public CalendarDayView (CalendarMonthView mv)
		{
			_mv = mv;
			BackgroundColor = UIColor.White;
		}

		public override void Draw(RectangleF rect)
		{
			UIImage img = null;
			UIColor color = UIColor.Gray;
			
			if (!Active || !Available)
			{
				//color = UIColor.FromRGBA(0.576f, 0.608f, 0.647f, 1f);
				//img = UIImage.FromBundle("Images/Calendar/datecell.png");
			} 
			else if (Today && Selected)
			{
				//color = UIColor.White;
				img = UIImage.FromBundle("Images/Calendar/todayselected.png").CreateResizableImage(new UIEdgeInsets(4,4,4,4));
			} 
			else if (Today)
			{
				//color = UIColor.White;
				img = UIImage.FromBundle("Images/Calendar/today.png").CreateResizableImage(new UIEdgeInsets(4,4,4,4));
			} 
			else if (Selected || Marked)
			{
				//color = UIColor.White;
				img = UIImage.FromBundle("Images/Calendar/datecellselected.png").CreateResizableImage(new UIEdgeInsets(4,4,4,4));
			}
			else
			{
				color = UIColor.FromRGBA(0.275f, 0.341f, 0.412f, 1f);
				//img = UIImage.FromBundle("Images/Calendar/datecell.png");
			}

			if (img != null)
				img.Draw(new RectangleF(0, 0, _mv.BoxWidth, _mv.BoxHeight));

			color.SetColor();
			var inflated = new RectangleF(0, 5, Bounds.Width, Bounds.Height);
			DrawString(Text, inflated,
						UIFont.BoldSystemFontOfSize(16), 
			           UILineBreakMode.WordWrap, UITextAlignment.Center);
			
			//            if (Marked)
			//            {
			//                var context = UIGraphics.GetCurrentContext();
			//                if (Selected || Today)
			//                    context.SetRGBFillColor(1, 1, 1, 1);
			//                else if (!Active || !Available)
			//					UIColor.LightGray.SetColor();
			//				else
			//                    context.SetRGBFillColor(75/255f, 92/255f, 111/255f, 1);
			//                context.SetLineWidth(0);
			//                context.AddEllipseInRect(new RectangleF(Frame.Size.Width/2 - 2, 45-10, 4, 4));
			//                context.FillPath();
			//
			//            }
		}
	}
}

