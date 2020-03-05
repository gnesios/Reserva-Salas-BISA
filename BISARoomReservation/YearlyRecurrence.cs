using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BISARoomReservation
{
    class YearlyRecurrence
    {
        int yearFrequency; //years between events
        int monthOfTheYear; //month of the year
        int dayOfTheMonth; //day of the month

        DateTime currentStartDate;
        DateTime currentEndDate;

        List<CalendarEvent> theCalendarEvents;

        public List<CalendarEvent> TheCalendarEvents
        {
            get { return theCalendarEvents; }
            set { theCalendarEvents = value; }
        }

        public YearlyRecurrence(DateTime startDate, DateTime endDate, XmlNode repeatNode, XmlNode recurrenceNode)
        {
            DateTime initialStartDate = startDate;
            DateTime initialEndDate = endDate;

            theCalendarEvents = new List<CalendarEvent>();

            Int32.TryParse(repeatNode.Attributes["yearFrequency"].Value, out yearFrequency);
            Int32.TryParse(repeatNode.Attributes["month"].Value, out monthOfTheYear);
            Int32.TryParse(repeatNode.Attributes["day"].Value, out dayOfTheMonth);

            initialStartDate = new DateTime(initialStartDate.Year, monthOfTheYear, dayOfTheMonth,
                initialStartDate.Hour, initialStartDate.Minute, initialStartDate.Second);
            initialEndDate = new DateTime(initialEndDate.Year, monthOfTheYear, dayOfTheMonth,
                initialEndDate.Hour, initialEndDate.Minute, initialEndDate.Second);

            DateTime revisedEndDate = new DateTime(initialStartDate.Year, initialStartDate.Month,
                initialStartDate.Day, initialEndDate.Hour, initialEndDate.Minute, initialEndDate.Second);

            if (initialStartDate < startDate)
            {
                currentStartDate = initialStartDate.AddYears(yearFrequency);
                currentEndDate = revisedEndDate.AddYears(yearFrequency);
            }
            else
            {
                currentStartDate = initialStartDate;
                currentEndDate = revisedEndDate;
            }

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
            currentStartDate = currentStartDate.AddYears(yearFrequency);
            currentEndDate = currentEndDate.AddYears(yearFrequency);

            if (currentStartDate.Day != dayOfTheMonth || currentStartDate.Month != monthOfTheYear)
            {
                try
                {
                    currentStartDate = new DateTime(currentStartDate.Year, monthOfTheYear, dayOfTheMonth,
                        currentStartDate.Hour, currentStartDate.Minute, currentStartDate.Second);
                    currentEndDate = new DateTime(currentEndDate.Year, monthOfTheYear, dayOfTheMonth,
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
                newEndDate = currentEndDate.AddYears(5); //quit after 5 years
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
