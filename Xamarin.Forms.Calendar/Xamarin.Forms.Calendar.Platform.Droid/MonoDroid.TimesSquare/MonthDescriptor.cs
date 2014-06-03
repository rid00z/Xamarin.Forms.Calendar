using System;

namespace Xamarin.Forms.Calendar.Platform.Droid
{
    public class MonthDescriptor
    {
        public int Month { get; private set; }
        public int Year { get; private set; }
        public DateTime Date { get; private set; }
        public string Label { get; private set; }

        public MonthDescriptor(int month, int year, DateTime date, string label)
        {
            Month = month;
            Year = year;
            Date = date;
            Label = label;
        }

        public override string ToString()
        {
            return "MonthDescriptor{"
                   + "label=" + Label + ""
                   + ", month=" + Month
                   + ", year=" + Year
                   + "}";
        }
    }
}