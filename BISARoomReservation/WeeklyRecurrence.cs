using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BISARoomReservation
{
    class WeeklyRecurrence
    {
        int weekFrequency; //weeks between events
        bool[] daysOfTheWeek; //days of the week

        DayOfWeek currentDayOfWeek;
        DateTime currentStartBeginOfWeek;
        DateTime currentEndBeginOfWeek;

        DateTime currentStartDate;
        DateTime currentEndDate;

        List<CalendarEvent> theCalendarEvents;

        public List<CalendarEvent> TheCalendarEvents
        {
            get { return theCalendarEvents; }
            set { theCalendarEvents = value; }
        }

        public WeeklyRecurrence(DateTime startDate, DateTime endDate, XmlNode repeatNode, XmlNode recurrenceNode)
        {
            weekFrequency = 0;
            daysOfTheWeek = new bool[7];

            currentDayOfWeek = DayOfWeek.Monday;
            currentStartBeginOfWeek = DateTime.MinValue;
            currentEndBeginOfWeek = DateTime.MinValue;
            currentStartDate = startDate;
            currentEndDate = endDate;

            theCalendarEvents = new List<CalendarEvent>();

            for (int counter = 0; counter < repeatNode.Attributes.Count; counter++)
            {
                string attrName = repeatNode.Attributes[counter].Name;
                string attrValue = repeatNode.Attributes[counter].Value;

                bool theAttrValue = false;
                Boolean.TryParse(attrValue, out theAttrValue);

                switch (attrName)
                {
                    case "weekFrequency":
                        Int32.TryParse(attrValue, out weekFrequency);
                        break;
                    case "su":
                        daysOfTheWeek[(int)DayOfWeek.Sunday] = theAttrValue;
                        break;
                    case "mo":
                        daysOfTheWeek[(int)DayOfWeek.Monday] = theAttrValue;
                        break;
                    case "tu":
                        daysOfTheWeek[(int)DayOfWeek.Tuesday] = theAttrValue;
                        break;
                    case "we":
                        daysOfTheWeek[(int)DayOfWeek.Wednesday] = theAttrValue;
                        break;
                    case "th":
                        daysOfTheWeek[(int)DayOfWeek.Thursday] = theAttrValue;
                        break;
                    case "fr":
                        daysOfTheWeek[(int)DayOfWeek.Friday] = theAttrValue;
                        break;
                    case "sa":
                        daysOfTheWeek[(int)DayOfWeek.Saturday] = theAttrValue;
                        break;
                }
            }

            //adjust start date to first selected day of week
            currentDayOfWeek = startDate.DayOfWeek;
            if (!daysOfTheWeek[(int)currentDayOfWeek])
            {
                int counter = (int)currentDayOfWeek;
                while (counter < 7 && !daysOfTheWeek[counter])
                {
                    counter++;
                    currentStartDate = currentStartDate.AddDays(1);
                    currentEndDate = currentEndDate.AddDays(1);

                    if (counter == 7)
                    {
                        counter = 0;

                        if (weekFrequency > 1)
                        {
                            currentStartDate = currentStartDate.AddDays((weekFrequency - 1) * 7);
                            currentEndDate = currentEndDate.AddDays((weekFrequency -1) * 7);
                        }
                    }
                }

                currentDayOfWeek = currentStartDate.DayOfWeek;
            }

            DateTime revisedEndDate = new DateTime(currentStartDate.Year, currentStartDate.Month,
                currentStartDate.Day, currentEndDate.Hour, currentEndDate.Minute, currentEndDate.Second);
            currentEndDate = revisedEndDate;

            currentStartBeginOfWeek = currentStartDate;
            currentEndBeginOfWeek = currentEndDate;

            //Need to adjust currentStartBeginOfWeek to the first day
            //this week so when the week gets incremented in incrementDates
            //it includes days of the week before today.
            bool foundEarlier = false;
            int firstDayOfWeek = 7;
            for (int counter = (int)currentStartDate.DayOfWeek; counter >= 0; counter--)
            {
                if (daysOfTheWeek[counter])
                {
                    firstDayOfWeek = counter;
                    foundEarlier = true;
                }
            }

            if (foundEarlier)
            {
                int deltaDays = firstDayOfWeek - (int)currentStartDate.DayOfWeek;
                currentStartBeginOfWeek = currentStartBeginOfWeek.AddDays(deltaDays);
                currentEndBeginOfWeek = currentEndBeginOfWeek.AddDays(deltaDays);
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
            currentStartDate = currentStartDate.AddDays(1);
            currentEndDate = currentEndDate.AddDays(1);

            int counter = (int)currentDayOfWeek + 1;
            while (counter < 7 && !daysOfTheWeek[counter])
            {
                counter++;
                currentStartDate = currentStartDate.AddDays(1);
                currentEndDate = currentEndDate.AddDays(1);
            }

            currentDayOfWeek = currentStartDate.DayOfWeek;

            //new week
            if (counter == 7)
            {
                currentStartBeginOfWeek = currentStartBeginOfWeek.AddDays(weekFrequency * 7);
                currentEndBeginOfWeek = currentEndBeginOfWeek.AddDays(weekFrequency * 7);

                currentStartDate = currentStartBeginOfWeek;
                currentEndDate = currentEndBeginOfWeek;
                currentDayOfWeek = currentStartDate.DayOfWeek;
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
