using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BISARoomReservation
{
    static class RecurrenceEventsGenerator
    {
        static List<CalendarEvent> newCalendarEvents = new List<CalendarEvent>();

        static DailyRecurrence dailyRecurrence = null;
        static WeeklyRecurrence weeklyRecurrence = null;
        static MonthlyRecurrence monthlyRecurrence = null;
        static YearlyRecurrence yearlyRecurrence = null;

        static XmlNode firstDayOfWeekNode;
        static XmlNode repeatNode;
        static XmlNode recurrenceNode;

        static public List<CalendarEvent> GetAllRecurrenceEvents(CalendarEvent calendarEvent)
        {
            ProcessRecurrence(calendarEvent.EventDate, calendarEvent.EndDate, calendarEvent.RecurrenceData);

            return newCalendarEvents;
        }

        static private void ProcessRecurrence(DateTime startDate, DateTime endDate, string recurrenceXml)
        {
            ParseXML(recurrenceXml);

            XmlNode typeRepeatNode = repeatNode.FirstChild;
            string typeRepeat = typeRepeatNode.Name;

            //Create the repeating handler based on the typeRepeat node name
            switch (typeRepeat)
            {
                case "daily":
                    dailyRecurrence = new DailyRecurrence(startDate, endDate, typeRepeatNode, recurrenceNode);
                    newCalendarEvents = dailyRecurrence.TheCalendarEvents;
                    break;
                case "weekly":
                    weeklyRecurrence = new WeeklyRecurrence(startDate, endDate, typeRepeatNode, recurrenceNode);
                    newCalendarEvents = weeklyRecurrence.TheCalendarEvents;
                    break;
                case "monthly":
                    monthlyRecurrence = new MonthlyRecurrence(startDate, endDate, typeRepeatNode, recurrenceNode);
                    newCalendarEvents = monthlyRecurrence.TheCalendarEvents;
                    break;
                case "yearly":
                    yearlyRecurrence = new YearlyRecurrence(startDate, endDate, typeRepeatNode, recurrenceNode);
                    newCalendarEvents = yearlyRecurrence.TheCalendarEvents;
                    break;
            }
        }

        static private void ParseXML(string recurrenceXml)
        {
            XmlDocument doc = new XmlDocument();
            XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(recurrenceXml));
            XmlNode node = doc.ReadNode(reader);
            reader.Close();

            //extract the <rule> node
            XmlNode ruleNode = node.FirstChild;
            while (ruleNode.NodeType == XmlNodeType.Whitespace)
            {
                ruleNode = ruleNode.NextSibling;
            }

            //extract the <firstDayOfWeek> node
            firstDayOfWeekNode = ruleNode.FirstChild;
            while (firstDayOfWeekNode.NodeType == XmlNodeType.Whitespace)
            {
                firstDayOfWeekNode = firstDayOfWeekNode.NextSibling;
            }

            //get the <repeat> node
            repeatNode = ruleNode["repeat"];

            //get the <repeatInstances> node
            recurrenceNode = repeatNode.NextSibling;
            while (recurrenceNode.NodeType == XmlNodeType.Whitespace)
            {
                recurrenceNode = recurrenceNode.NextSibling;
            }
        }
    }
}
