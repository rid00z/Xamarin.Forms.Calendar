using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using Android.Content;
using Android.Util;
using Android.Widget;
using Java.Lang;
using DroidListView = Android.Widget.ListView;

namespace Xamarin.Forms.Calendar.Platform.Droid
{
	public class CalendarPickerView : DroidListView
    {
        public enum SelectionMode
        {
            Single,
            Multi,
            Range
        };

        private readonly Context _context;
        internal readonly MonthAdapter MyAdapter;
        internal readonly List<MonthDescriptor> Months = new List<MonthDescriptor>();

        internal readonly List<List<List<MonthCellDescriptor>>> Cells =
            new List<List<List<MonthCellDescriptor>>>();

        internal List<MonthCellDescriptor> SelectedCells = new List<MonthCellDescriptor>();
        private readonly List<MonthCellDescriptor> _highlightedCells = new List<MonthCellDescriptor>();
        internal List<DateTime> SelectedCals = new List<DateTime>();
        private readonly List<DateTime> _highlightedCals = new List<DateTime>();
        internal readonly DateTime Today = DateTime.Now;
        internal DateTime MinDate;
        internal DateTime MaxDate;
        private DateTime _monthCounter;

        internal readonly string MonthNameFormat;
        internal readonly string WeekdayNameFormat;
        internal readonly string FullDateFormat;

        internal ClickHandler ClickHandler;

        public event EventHandler<DateSelectedEventArgs> OnInvalidDateSelected;
        public event EventHandler<DateSelectedEventArgs> OnDateSelected;
        public event EventHandler<DateSelectedEventArgs> OnDateUnselected;
        public event DateSelectableHandler OnDateSelectable;

        public SelectionMode Mode { get; set; }

        public DateTime SelectedDate
        {
            get { return SelectedCals.Count > 0 ? SelectedCals[0] : DateTime.MinValue; }
        }

        public List<DateTime> SelectedDates
        {
            get
            {
                var selectedDates = SelectedCells.Select(cal => cal.DateTime).ToList();
                selectedDates.Sort();
                return selectedDates;
            }
        }

        public CalendarPickerView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            ResourceIdManager.UpdateIdValues();
            _context = context;
            MyAdapter = new MonthAdapter(context, this);
            base.Adapter = MyAdapter;
            base.Divider = null;
            base.DividerHeight = 0;

            var bgColor = base.Resources.GetColor(Resource.Color.calendar_bg);
            base.SetBackgroundColor(bgColor);
            base.CacheColorHint = bgColor;

            MonthNameFormat = base.Resources.GetString(Resource.String.month_name_format);
            WeekdayNameFormat = base.Resources.GetString(Resource.String.day_name_format);
            FullDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            ClickHandler += OnCellClicked;
            OnInvalidDateSelected += OnInvalidateDateClicked;

            if (base.IsInEditMode) {
                Init(DateTime.Now, DateTime.Now.AddYears(1)).WithSelectedDate(DateTime.Now);
            }
        }

        private void OnCellClicked(MonthCellDescriptor cell)
        {
            var clickedDate = cell.DateTime;

            if (!IsBetweenDates(clickedDate, MinDate, MaxDate) || !IsSelectable(clickedDate)) {
                if (OnInvalidDateSelected != null) {
                    OnInvalidDateSelected(this, new DateSelectedEventArgs(clickedDate));
                }
            }
            else {
                bool wasSelected = DoSelectDate(clickedDate, cell);
                if (OnDateSelected != null) {
                    if (wasSelected) {
                        OnDateSelected(this, new DateSelectedEventArgs(clickedDate));
                    }
                    else if (OnDateUnselected != null) {
                        OnDateUnselected(this, new DateSelectedEventArgs(clickedDate));
                    }
                }
            }
        }

        private void OnInvalidateDateClicked(object sender, DateSelectedEventArgs e)
        {
            string fullDateFormat = _context.Resources.GetString(Resource.String.full_date_format);
            string errorMsg = _context.Resources.GetString(Resource.String.invalid_date);
            errorMsg = string.Format(errorMsg, MinDate.ToString(fullDateFormat),
                MaxDate.ToString(fullDateFormat));
            Toast.MakeText(_context, errorMsg, ToastLength.Short).Show();
        }

        public FluentInitializer Init(DateTime minDate, DateTime maxDate)
        {
            if (minDate == DateTime.MinValue || maxDate == DateTime.MinValue) {
                throw new IllegalArgumentException("minDate and maxDate must be non-zero. " +
                                                   Debug(minDate, maxDate));
            }
            if (minDate.CompareTo(maxDate) > 0) {
                throw new IllegalArgumentException("minDate must be before maxDate. " +
                                                   Debug(minDate, maxDate));
            }

            Mode = SelectionMode.Single;
            //Clear out any previously selected dates/cells.
            SelectedCals.Clear();
            SelectedCells.Clear();
            _highlightedCals.Clear();
            _highlightedCells.Clear();

            //Clear previous state.
            Cells.Clear();
            Months.Clear();
            MinDate = minDate;
            MaxDate = maxDate;
            MinDate = SetMidnight(MinDate);
            MaxDate = SetMidnight(MaxDate);

            // maxDate is exclusive: bump back to the previous day so if maxDate is the first of a month,
            // We don't accidentally include that month in the view.
            MaxDate = MaxDate.AddMinutes(-1);

            //Now iterate between minCal and maxCal and build up our list of months to show.
            _monthCounter = MinDate;
            int maxMonth = MaxDate.Month;
            int maxYear = MaxDate.Year;
            while ((_monthCounter.Month <= maxMonth
                    || _monthCounter.Year < maxYear)
                   && _monthCounter.Year < maxYear + 1) {
                var month = new MonthDescriptor(_monthCounter.Month, _monthCounter.Year, _monthCounter,
                    _monthCounter.ToString(MonthNameFormat));
                Cells.Add(GetMonthCells(month, _monthCounter));
                Logr.D("Adding month {0}", month);
                Months.Add(month);
                _monthCounter = _monthCounter.AddMonths(1);
            }

            ValidateAndUpdate();
            return new FluentInitializer(this);
        }

        internal List<List<MonthCellDescriptor>> GetMonthCells(MonthDescriptor month, DateTime startCal)
        {
            var cells = new List<List<MonthCellDescriptor>>();
            var cal = new DateTime(startCal.Year, startCal.Month, 1);
            var firstDayOfWeek = (int) cal.DayOfWeek;
            cal = cal.AddDays((int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - firstDayOfWeek);

            var minSelectedCal = GetMinDate(SelectedCals);
            var maxSelectedCal = GetMaxDate(SelectedCals);

            while ((cal.Month < month.Month + 1 || cal.Year < month.Year)
                   && cal.Year <= month.Year) {
                Logr.D("Building week row starting at {0}", cal);
                var weekCells = new List<MonthCellDescriptor>();
                cells.Add(weekCells);
                for (int i = 0; i < 7; i++) {
                    var date = cal;
                    bool isCurrentMonth = cal.Month == month.Month;
                    bool isSelected = isCurrentMonth && ContatinsDate(SelectedCals, cal);
                    bool isSelectable = isCurrentMonth && IsBetweenDates(cal, MinDate, MaxDate);
                    bool isToday = IsSameDate(cal, Today);
                    bool isHighlighted = ContatinsDate(_highlightedCals, cal);
                    int value = cal.Day;

                    var rangeState = RangeState.None;
                    if (SelectedCals.Count > 1) {
                        if (IsSameDate(minSelectedCal, cal)) {
                            rangeState = RangeState.First;
                        }
                        else if (IsSameDate(maxSelectedCal, cal)) {
                            rangeState = RangeState.Last;
                        }
                        else if (IsBetweenDates(cal, minSelectedCal, maxSelectedCal)) {
                            rangeState = RangeState.Middle;
                        }
                    }

                    weekCells.Add(new MonthCellDescriptor(date, isCurrentMonth, isSelectable, isSelected,
                        isToday, isHighlighted, value, rangeState));
                    cal = cal.AddDays(1);
                }
            }
            return cells;
        }

        internal void ScrollToSelectedMonth(int selectedIndex)
        {
            ScrollToSelectedMonth(selectedIndex, false);
        }

        internal void ScrollToSelectedMonth(int selectedIndex, bool smoothScroll)
        {
            Task.Factory.StartNew(() =>
            {
                if (smoothScroll) {
                    SmoothScrollToPosition(selectedIndex);
                }
                else {
                    SetSelection(selectedIndex);
                }
            });
        }

        private MonthCellWithMonthIndex GetMonthCellWithIndexByDate(DateTime date)
        {
            int index = 0;

            foreach (var monthCell in Cells) {
                foreach (var actCell in from weekCell in monthCell
                    from actCell in weekCell
                    where IsSameDate(actCell.DateTime, date) && actCell.IsSelectable
                    select actCell)
                    return new MonthCellWithMonthIndex(actCell, index);
                index++;
            }
            return null;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (Months.Count == 0) {
                throw new InvalidOperationException(
                    "Must have at least one month to display. Did you forget to call Init()?");
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        private static DateTime SetMidnight(DateTime date)
        {
            return date.Subtract(date.TimeOfDay);
        }

        private bool IsSelectable(DateTime date)
        {
            return OnDateSelectable == null || OnDateSelectable(date);
        }

        private DateTime ApplyMultiSelect(DateTime date, DateTime selectedCal)
        {
            foreach (var selectedCell in SelectedCells) {
                if (selectedCell.DateTime == date) {
                    //De-select the currently selected cell.
                    selectedCell.IsSelected = false;
                    SelectedCells.Remove(selectedCell);
                    date = DateTime.MinValue;
                    break;
                }
            }

            foreach (var cal in SelectedCals) {
                if (IsSameDate(cal, selectedCal)) {
                    SelectedCals.Remove(cal);
                    break;
                }
            }
            return date;
        }

        private void ClearOldSelection()
        {
            foreach (var selectedCell in SelectedCells) {
                //De-select the currently selected cell.
                selectedCell.IsSelected = false;
            }
            SelectedCells.Clear();
            SelectedCals.Clear();
        }

        internal bool DoSelectDate(DateTime date, MonthCellDescriptor cell)
        {
            var newlySelectedDate = date;
            SetMidnight(newlySelectedDate);

            //Clear any remaining range state.
            foreach (var selectedCell in SelectedCells) {
                selectedCell.RangeState = RangeState.None;
            }

            switch (Mode) {
                case SelectionMode.Range:
                    if (SelectedCals.Count > 1) {
                        //We've already got a range selected: clear the old one.
                        ClearOldSelection();
                    }
                    else if (SelectedCals.Count == 1 && newlySelectedDate.CompareTo(SelectedCals[0]) < 0) {
                        //We're moving the start of the range back in time: clear the old start date.
                        ClearOldSelection();
                    }
                    break;
                case SelectionMode.Multi:
                    date = ApplyMultiSelect(date, newlySelectedDate);
                    break;
                case SelectionMode.Single:
                    ClearOldSelection();
                    break;
                default:
                    throw new IllegalStateException("Unknown SelectionMode " + Mode);
            }

            if (date > DateTime.MinValue) {
                if (SelectedCells.Count == 0 || !SelectedCells[0].Equals(cell)) {
                    SelectedCells.Add(cell);
                    cell.IsSelected = true;
                }
                SelectedCals.Add(newlySelectedDate);

                if (Mode == SelectionMode.Range && SelectedCells.Count > 1) {
                    //Select all days in between start and end.
                    var startDate = SelectedCells[0].DateTime;
                    var endDate = SelectedCells[1].DateTime;
                    SelectedCells[0].RangeState = RangeState.First;
                    SelectedCells[1].RangeState = RangeState.Last;

                    foreach (var month in Cells) {
                        foreach (var week in month) {
                            foreach (var singleCell in week) {
                                var singleCellDate = singleCell.DateTime;
                                if (singleCellDate.CompareTo(startDate) >= 0
                                    && singleCellDate.CompareTo(endDate) <= 0
                                    && singleCell.IsSelectable) {
                                    singleCell.IsSelected = true;
                                    singleCell.RangeState = RangeState.Middle;
                                    SelectedCells.Add(singleCell);
                                }
                            }
                        }
                    }
                }
            }
            ValidateAndUpdate();
            return date > DateTime.MinValue;
        }

        internal void ValidateAndUpdate()
        {
            if (Adapter == null) {
                Adapter = MyAdapter;
            }
            MyAdapter.NotifyDataSetChanged();
        }

        internal bool SelectDate(DateTime date)
        {
            return SelectDate(date, false);
        }

        private bool SelectDate(DateTime date, bool smoothScroll)
        {
            ValidateDate(date);

            var cell = GetMonthCellWithIndexByDate(date);
            if (cell == null || !IsSelectable(date)) {
                return false;
            }

            bool wasSelected = DoSelectDate(date, cell.Cell);
            if (wasSelected) {
                ScrollToSelectedMonth(cell.MonthIndex, smoothScroll);
            }
            return wasSelected;
        }

        private void ValidateDate(DateTime date)
        {
            if (date == DateTime.MinValue) {
                throw new IllegalArgumentException("Selected date must be non-zero.");
            }
            if (date.CompareTo(MinDate) < 0 || date.CompareTo(MaxDate) > 0) {
                throw new IllegalArgumentException(
                    string.Format("Selected date must be between minDate and maxDate. "
                                  + "minDate: {0}, maxDate: {1}, selectedDate: {2}.",
                        MinDate.ToShortDateString(), MaxDate.ToShortDateString(), date.ToShortDateString()));
            }
        }

        private static DateTime GetMinDate(List<DateTime> selectedCals)
        {
            if (selectedCals == null || selectedCals.Count == 0) {
                return DateTime.MinValue;
            }
            selectedCals.Sort();
            return selectedCals[0];
        }

        private static DateTime GetMaxDate(List<DateTime> selectedCals)
        {
            if (selectedCals == null || selectedCals.Count == 0) {
                return DateTime.MinValue;
            }
            selectedCals.Sort();
            return selectedCals[selectedCals.Count - 1];
        }

        private static bool IsBetweenDates(DateTime date, DateTime minCal, DateTime maxCal)
        {
            return (date.Equals(minCal) || date.CompareTo(minCal) > 0) // >= minCal
                   && date.CompareTo(maxCal) < 0; // && < maxCal
        }

        private static bool IsSameDate(DateTime cal, DateTime selectedDate)
        {
            return cal.Month == selectedDate.Month
                   && cal.Year == selectedDate.Year
                   && cal.Day == selectedDate.Day;
        }

        internal static bool IsSameMonth(DateTime cal, MonthDescriptor month)
        {
            return (cal.Month == month.Month && cal.Year == month.Year);
        }

        private static bool ContatinsDate(IEnumerable<DateTime> selectedCals, DateTime cal)
        {
            return selectedCals.Any(selectedCal => IsSameDate(cal, selectedCal));
        }

        public void HighlightDates(ICollection<DateTime> dates)
        {
            foreach (var date in dates) {
                ValidateDate(date);

                var monthCellWithMonthIndex = GetMonthCellWithIndexByDate(date);
                if (monthCellWithMonthIndex != null) {
                    var cell = monthCellWithMonthIndex.Cell;
                    _highlightedCells.Add(cell);
                    _highlightedCals.Add(date);
                    cell.IsHighlighted = true;
                }
            }

            MyAdapter.NotifyDataSetChanged();
            Adapter = MyAdapter;
        }

        private static string Debug(DateTime minDate, DateTime maxDate)
        {
            return "minDate: " + minDate + "\nmaxDate: " + maxDate;
        }

        private class MonthCellWithMonthIndex
        {
            public readonly MonthCellDescriptor Cell;
            public readonly int MonthIndex;

            public MonthCellWithMonthIndex(MonthCellDescriptor cell, int monthIndex)
            {
                Cell = cell;
                MonthIndex = monthIndex;
            }
        }
    }

    public class FluentInitializer
    {
        private readonly CalendarPickerView _calendar;

        public FluentInitializer(CalendarPickerView calendar)
        {
            _calendar = calendar;
        }

        public FluentInitializer InMode(CalendarPickerView.SelectionMode mode)
        {
            _calendar.Mode = mode;
            _calendar.ValidateAndUpdate();
            return this;
        }

        public FluentInitializer WithSelectedDate(DateTime selectedDate)
        {
            return WithSelectedDates(new List<DateTime> {selectedDate});
        }

        public FluentInitializer WithSelectedDates(ICollection<DateTime> selectedDates)
        {
            if (_calendar.Mode == CalendarPickerView.SelectionMode.Single && _calendar.SelectedDates.Count > 1) {
                throw new IllegalArgumentException("SINGLE mode can't be used with multiple selectedDates");
            }
            if (_calendar.SelectedDates != null) {
                foreach (var date in selectedDates) {
                    _calendar.SelectDate(date);
                }
            }
            int selectedIndex = -1;
            int todayIndex = -1;
            for (int i = 0; i < _calendar.Months.Count; i++) {
                var month = _calendar.Months[i];
                if (selectedIndex == -1) {
                    if (_calendar.SelectedCals.Any(
                        selectedCal => CalendarPickerView.IsSameMonth(selectedCal, month))) {
                        selectedIndex = i;
                    }
                    if (selectedIndex == -1 && todayIndex == -1 &&
                        CalendarPickerView.IsSameMonth(DateTime.Now, month)) {
                        todayIndex = i;
                    }
                }
            }
            if (selectedIndex != -1) {
                _calendar.ScrollToSelectedMonth(selectedIndex);
            }
            else if (todayIndex != -1) {
                _calendar.ScrollToSelectedMonth(todayIndex);
            }

            _calendar.ValidateAndUpdate();
            return this;
        }

        public FluentInitializer WithLocale(Java.Util.Locale locale)
        {
            //Not sure how to translate this to C# flavor.
            //Leave it later.
            throw new NotImplementedException();
        }

        public FluentInitializer WithHighlightedDates(ICollection<DateTime> dates)
        {
            _calendar.HighlightDates(dates);
            return this;
        }

        public FluentInitializer WithHighlightedDate(DateTime date)
        {
            return WithHighlightedDates(new List<DateTime> {date});
        }
    }

    public delegate void ClickHandler(MonthCellDescriptor cell);

    public delegate bool DateSelectableHandler(DateTime date);

    public class DateSelectedEventArgs : EventArgs
    {
        public DateSelectedEventArgs(DateTime date)
        {
            SelectedDate = date;
        }

        public DateTime SelectedDate { get; private set; }
    }
}