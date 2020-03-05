using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BISARoomReservation
{
    class DailyRecurrence
    {
        int dayFrequency; //how many days between events
        bool weekDays; //restrict events to weekdays only

        DateTime currentStartDate;
        DateTime currentEndDate;

        List<CalendarEvent> theCalendarEvents;

        public List<CalendarEvent> TheCalendarEvents
        {
          get { return theCalendarEvents; }
          set { theCalendarEvents = value; }
        }

        public DailyRecurrence(DateTime startDate, DateTime endDate, XmlNode repeatNode, XmlNode recurrenceNode)
        {
            dayFrequency = 0;
            weekDays = false;
            currentStartDate = startDate;
            currentEndDate = endDate;

            theCalendarEvents = new List<CalendarEvent>();

            //repeatNode contains either dayFrequency or weekday
            if (repeatNode.Attributes[0].Name == "dayFrequency")
            {
                string attributeValue = repeatNode.Attributes[0].Value;
                Int32.TryParse(attributeValue, out dayFrequency);
            }
            else if (repeatNode.Attributes[0].Name == "weekday")
            {
                dayFrequency = 1;
                weekDays = true;
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

        private void GenerateTheCalendarEvents(int theRepeatInstances)
        {
            for (int i = 0; i < theRepeatInstances; i++)
            {
                this. WeekdayValidation();

                CalendarEvent newCalendarEvent = new CalendarEvent(currentStartDate, currentEndDate);
                theCalendarEvents.Add(newCalendarEvent);

                this.IncrementDates();
            }
        }

        private void GenerateTheCalendarEvents(DateTime theRepeatUntilDate)
        {
            do
            {
                this.WeekdayValidation();

                CalendarEvent newCalendarEvent = new CalendarEvent(currentStartDate, currentEndDate);
                theCalendarEvents.Add(newCalendarEvent);

                this.IncrementDates();
            } while (currentEndDate <= theRepeatUntilDate);
        }

        private void IncrementDates()
        {
            currentStartDate = currentStartDate.AddDays(dayFrequency);
            currentEndDate = currentEndDate.AddDays(dayFrequency);
        }

        private void WeekdayValidation()
        {
            if (weekDays)
            {
                if (currentStartDate.DayOfWeek == DayOfWeek.Saturday)
                    currentStartDate = currentStartDate.AddDays(2);
                else if (currentStartDate.DayOfWeek == DayOfWeek.Sunday)
                    currentStartDate = currentStartDate.AddDays(1);

                if (currentEndDate.DayOfWeek == DayOfWeek.Saturday)
                    currentEndDate = currentEndDate.AddDays(2);
                else if (currentEndDate.DayOfWeek == DayOfWeek.Sunday)
                    currentEndDate = currentEndDate.AddDays(1);
            }
        }

        private int GetTheRepeatInstances(XmlNode recurrenceNode)
        {
            int theRepeatInstances = 0;

            XmlNode recurrenceValueNode = recurrenceNode.FirstChild;
            string recurrenceValue = recurrenceValueNode.Value;
            Int32.TryParse(recurrenceValue, out theRepeatInstances);

            return theRepeatInstances;
        }

        private DateTime GetTheRepeatUntil(XmlNode recurrenceNode, string recurrenceName)
        {
            DateTime theRepeatUntilDate;

            DateTime newEndDate;
            if (recurrenceName == "windowEnd")
            {
                string theEndDate = recurrenceNode.FirstChild.Value;
                newEndDate = Convert.ToDateTime(theEndDate);

                DateTime revisedEndDate = new DateTime(currentStartDate.Year, currentStartDate.Month,
                    currentStartDate.Day, newEndDate.Hour, newEndDate.Minute, newEndDate.Second);
                currentEndDate = revisedEndDate;
            }
            else
            {//repeatForever
                newEndDate = currentEndDate.AddYears(1); //quit after 1 year
            }
            
            theRepeatUntilDate = newEndDate;

            return theRepeatUntilDate;
        }
    }
}
