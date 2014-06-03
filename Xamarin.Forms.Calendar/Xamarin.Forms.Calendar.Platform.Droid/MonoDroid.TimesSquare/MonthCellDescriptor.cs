using System;

namespace Xamarin.Forms.Calendar.Platform.Droid
{
    public enum RangeState
    {
        None,
        First,
        Middle,
        Last
    }

    public class MonthCellDescriptor : Java.Lang.Object
    {
        public DateTime DateTime { get; set; }
        public int Value { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsSelected { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelectable { get; set; }
        public bool IsHighlighted { get; set; }
        public RangeState RangeState { get; set; }

        public MonthCellDescriptor(DateTime date, bool isCurrentMonth, bool isSelectable, bool isSelected,
            bool isToday, bool isHighlighted, int value, RangeState rangeState)
        {
            DateTime = date;
            Value = value;
            IsCurrentMonth = isCurrentMonth;
            IsSelected = isSelected;
            IsHighlighted = isHighlighted;
            IsToday = isToday;
            IsSelectable = isSelectable;
            RangeState = rangeState;
        }

        public override string ToString()
        {
            return "MonthCellDescriptor{"
                   + "Date=" + DateTime
                   + ", Value=" + Value
                   + ", IsCurrentMonth=" + IsCurrentMonth
                   + ", IsSelected=" + IsSelected
                   + ", IsToday=" + IsToday
                   + ", IsSelectable=" + IsSelectable
                   + ", IsHighlighted=" + IsHighlighted
                   + ", RangeSTate=" + RangeState
                   + "}";
        }
    }
}