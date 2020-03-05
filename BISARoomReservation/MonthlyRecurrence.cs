using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BISARoomReservation
{
    class MonthlyRecurrence
    {
        int monthFrequency; //months between events
        int dayOfTheMonth; //day of the month to appear

        DateTime currentStartDate;
        DateTime currentEndDate;

        List<CalendarEvent> theCalendarEvents;

        public List<CalendarEvent> TheCalendarEvents
        {
            get { return theCalendarEvents; }
            set { theCalendarEvents = value; }
        }

        public MonthlyRecurrence(DateTime startDate, DateTime endDate, XmlNode repeatNode, XmlNode recurrenceNode)
        {
            DateTime initialStartDate = startDate; 
            DateTime initialEndDate = endDate;

            string mFreq = repeatNode.Attributes["monthFrequency"].Value;
            string mDay = repeatNode.Attributes["day"].Value;

            Int32.TryParse(mFreq, out monthFrequency);
            Int32.TryParse(mDay, out dayOfTheMonth);

            initialStartDate = new DateTime(initialStartDate.Year, initialStartDate.Month, dayOfTheMonth,
                initialStartDate.Hour, initialStartDate.Minute, initialStartDate.Second);
            initialEndDate = new DateTime(initialEndDate.Year, initialEndDate.Month, dayOfTheMonth,
                initialEndDate.Hour, initialEndDate.Minute, initialEndDate.Second);

            DateTime revisedEndDate = new DateTime(initialStartDate.Year, initialStartDate.Month,
                initialStartDate.Day, initialEndDate.Hour, initialEndDate.Minute, initialEndDate.Second);

            if (initialStartDate < startDate)
            {
                currentStartDate = initialStartDate.AddMonths(monthFrequency);
                currentEndDate = revisedEndDate.AddMonths(monthFrequency);
            }
            else
            {
                currentStartDate = initialStartDate;
                currentEndDate = revisedEndDate;
            }

            theCalendarEvents = new List<CalendarEvent>();

            //
            string recurrenceName = recurrenceNode.Name;
            if (recurrenceName == "repeatInstances")
            {
                int theRepeatInstances = this.GetTheRepeatInstances(recurrenceNode);
                this.GenerateTheCalendarEvents(theRepeatInstances);
            }
            else if (recurrenceName == "repeatForever" || recurrenceName == "windowEnd")
            {
                DateTime theRepeatUntilDate = this.GetTheRepeatUntil(recurrenceNode, recurrenceName);
                this.GenerateTheCalendarEvents(theRepeatUntilDate);
            }
        }

        private void IncrementDates()
        {
            currentStartDate = currentStartDate.AddMonths(monthFrequency);
            currentEndDate = currentEndDate.AddMonths(monthFrequency);

            if (currentStartDate.Day != dayOfTheMonth)
            {
                try
                {
                    currentStartDate = new DateTime(currentStartDate.Year, currentStartDate.Month, dayOfTheMonth,
                        currentStartDate.Hour, currentStartDate.Minute, currentStartDate.Second);
                    currentEndDate = new DateTime(currentEndDate.Year, currentEndDate.Month, dayOfTheMonth,
                        currentEndDate.Hour, currentEndDate.Minute, currentEndDate.Second);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    //invalid date leaves the original calculation
                }
            }
        }

        #region Repeat Instances
        private int GetTheRepeatInstances(XmlNode recurrenceNode)
        {
            int theRepeatInstances = 0;

            XmlNode recurrenceValueNode = recurrenceNode.FirstChild;
            string recurrenceValue = recurrenceValueNode.Value;
            Int32.TryParse(recurrenceValue, out theRepeatInstances);

            return theRepeatInstances;
        }

        private void GenerateTheCalendarEvents(int theRepeatInstances)
        {
            for (int i = 0; i < theRepeatInstances; i++)
            {
                CalendarEvent newCalendarEvent = new CalendarEvent(currentStartDate, currentEndDate);
                theCalendarEvents.Add(newCalendarEvent);

                this.IncrementDates();
            }
        }
        #endregion

        #region Repeat Forever or Until
        private DateTime GetTheRepeatUntil(XmlNode recurrenceNode, string recurrenceName)
        {
            DateTime theRepeatUntilDate;

            DateTime newEndDate;
            if (recurrenceName == "windowEnd")
            {
                string theEndDate = recurrenceNode.FirstChild.Value;
                newEndDate = Convert.ToDateTime(theEndDate);
            }
            else
            {//repeatForever
                newEndDate = currentEndDate.AddYears(1); //quit after 1 year
            }

            theRepeatUntilDate = newEndDate;

            return theRepeatUntilDate;
        }

        private void GenerateTheCalendarEvents(DateTime theRepeatUntilDate)
        {
            do
            {
                CalendarEvent newCalendarEvent = new CalendarEvent(currentStartDate, currentEndDate);
                theCalendarEvents.Add(newCalendarEvent);

                this.IncrementDates();
            } while (currentEndDate <= theRepeatUntilDate);
        }
        #endregion
    }
}
