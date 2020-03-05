using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BISARoomReservation
{
    public class CalendarEvent
    {
        private DateTime eventDate;
        private DateTime endDate;
        private int isRecurrent;
        private int isAllDayEvent;
        private string recurrenceData;
        private string eventType;
        private int roomReserved;

        public DateTime EventDate
        {
            get { return eventDate; }
            set { eventDate = value; }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        public int IsRecurrent
        {
            get { return isRecurrent; }
            set { isRecurrent = value; }
        }
        public int IsAllDayEvent
        {
            get { return isAllDayEvent; }
            set { isAllDayEvent = value; }
        }
        public string RecurrenceData
        {
            get { return recurrenceData; }
            set { recurrenceData = value; }
        }
        public string EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }
        public int RoomReserved
        {
            get { return roomReserved; }
            set { roomReserved = value; }
        }

        #region Contructor
        public CalendarEvent()
        {
            EventDate = DateTime.MaxValue;
            EndDate = DateTime.MaxValue;
            IsRecurrent = 0;
            IsAllDayEvent = 0;
            RecurrenceData = string.Empty;
            EventType = string.Empty;
            RoomReserved = 0;
        }

        public CalendarEvent(DateTime startDate, DateTime endDate)
        {
            EventDate = startDate;
            EndDate = endDate;
        }
        #endregion
    }
}
