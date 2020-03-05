using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Collections.Generic;

namespace BISARoomReservation.ERCalendar
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ERCalendar : SPItemEventReceiver
    {
        const string SOURCE_LIST = "Reservas";
        const string ROOM_LIST = "Salas";
        const string RESOURCE_LIST = "Recursos";

        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);

            if (properties.ListTitle == SOURCE_LIST)
            {
                try
                {
                    if (this.ValidateOldReserve(properties))
                        this.ValidateNewReserve(properties);
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.CancelWithError;
                }
            }
        }

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);

            if (properties.ListTitle == SOURCE_LIST)
            {
                try
                {
                    if (this.ValidateOldReserve(properties))
                        this.ValidateChangeReserve(properties);
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.CancelWithError;
                }
            }
        }

        /// <summary>
        /// An item is being deleted.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);

            if (properties.ListTitle == SOURCE_LIST)
            {
                try
                {
                    this.ValidateDeleteReserve(properties);
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.CancelWithError;
                }
            }
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);

            if (properties.ListTitle == SOURCE_LIST)
            {
                try
                {
                    SPListItem theItem = properties.ListItem;
                    SPWeb theSite = properties.OpenWeb();
                    SPList theRoomList = theSite.Lists[ROOM_LIST];
                    SPList theResourceList = theSite.Lists[RESOURCE_LIST];

                    string roomNotificationMail = this.CheckForRoomNotification(theRoomList, theItem["Sala reserva"]);
                    List<string> resourceNotificationMails = this.CheckForResourceNotification(theResourceList, theItem["Recurso reserva"]);

                    #region Room notification
                    if (!string.IsNullOrWhiteSpace(roomNotificationMail))
                    {
                        /*if (!SPUtility.IsEmailServerSet(properties.Web))
                        {
                            throw new SPException("Outgoing E-Mail Settings are not configured!");
                        }
                        else
                        {*/
                            string room = this.SubstringFormat(theItem["Sala reserva"]);
                            string area = this.SubstringFormat(theItem["Área reserva"]);
                            string user = theItem["Usuario reserva"].ToString();
                            string subject = theItem.Title;
                            string begin = Convert.ToDateTime(theItem["Hora de inicio"].ToString()).ToString("dd/MM/yyyy HH:mm");
                            string end = Convert.ToDateTime(theItem["Hora de finalización"].ToString()).ToString("dd/MM/yyyy HH:mm");
                            string creator = this.SubstringFormat(theItem["Creado por"]);
                            string created = Convert.ToDateTime(theItem["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm");

                            System.Collections.Specialized.StringDictionary headers =
                                new System.Collections.Specialized.StringDictionary();
                            headers.Add("to", roomNotificationMail);
                            headers.Add("cc", "");
                            headers.Add("bcc", "");
                            headers.Add("from", "reservasalas@grupobisa.com");
                            headers.Add("subject", "Notificación automática, Sistema de Reserva de Salas BISA");
                            headers.Add("content-type", "text/html");
                            string bodyText = string.Format(
                                "<table border='0' cellspacing='0' cellpadding='0' style='width:99%;border-collapse:collapse;'>" +
                                "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                                "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                                "Sistema de Reserva de Salas: Reserva \"{0}\"</p></td></tr>" +
                                "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                                "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                                "Esta es una notificación automática generada por el <b>Sistema de Reserva de Salas</b> " +
                                "con el propósito de informar que se hizo la reserva siguiente:</p>" +
                                "<p>- Sala reservada: <b>{0}</b></p>" +
                                "<p>- Área solicitante: <b>{1}</b></p>" +
                                "<p>- Usuario solicitante: <b>{2}</b></p>" +
                                "<p>- Motivo: <b>{3}</b></p>" +
                                "<p>- Periodo: desde <b>{4}</b> hasta <b>{5}</b></p>" +
                                "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                                "Reserva creada por {6} en fecha {7}</p></td></tr></table>",
                                room, area, user, subject, begin, end, creator, created);

                            SPUtility.SendEmail(properties.Web, headers, bodyText);
                        /*}*/
                    }
                    #endregion

                    #region Resource notification
                    if (resourceNotificationMails.Count > 0)
                    {
                        /*if (!SPUtility.IsEmailServerSet(properties.Web))
                        {
                            throw new SPException("Outgoing E-Mail Settings are not configured!");
                        }
                        else
                        {*/
                            string room = this.SubstringFormat(theItem["Sala reserva"]);
                            string area = this.SubstringFormat(theItem["Área reserva"]);
                            string user = theItem["Usuario reserva"].ToString();
                            string subject = theItem.Title;
                            string begin = Convert.ToDateTime(theItem["Hora de inicio"].ToString()).ToString("dd/MM/yyyy HH:mm");
                            string end = Convert.ToDateTime(theItem["Hora de finalización"].ToString()).ToString("dd/MM/yyyy HH:mm");
                            string resources = this.GetFormatedResources(theItem["Recurso reserva"].ToString());
                            string creator = this.SubstringFormat(theItem["Creado por"]);
                            string created = Convert.ToDateTime(theItem["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm");

                            foreach (string resourceNotificationMail in resourceNotificationMails)
                            {
                                System.Collections.Specialized.StringDictionary headers =
                                new System.Collections.Specialized.StringDictionary();
                                headers.Add("to", resourceNotificationMail);
                                headers.Add("cc", "");
                                headers.Add("bcc", "");
                                headers.Add("from", "reservasalas@grupobisa.com");
                                headers.Add("subject", "Notificación automática, Sistema de Reserva de Salas BISA");
                                headers.Add("content-type", "text/html");
                                string bodyText = string.Format(
                                    "<table border='0' cellspacing='0' cellpadding='0' style='width:99%;border-collapse:collapse;'>" +
                                    "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                                    "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                                    "Sistema de Reserva de Salas: Reserva \"{0}\"</p></td></tr>" +
                                    "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                                    "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                                    "Esta es una notificación automática generada por el <b>Sistema de Reserva de Salas</b> " +
                                    "con el propósito de informar que se hizo la reserva siguiente:</p>" +
                                    "<p>- Sala reservada: <b>{0}</b></p>" +
                                    "<p>- Área solicitante: <b>{1}</b></p>" +
                                    "<p>- Usuario solicitante: <b>{2}</b></p>" +
                                    "<p>- Motivo: <b>{3}</b></p>" +
                                    "<p>- Periodo: desde <b>{4}</b> hasta <b>{5}</b></p>" +
                                    "<p>- Recursos a usar: <b>{6}</b></p>" +
                                    "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                                    "Reserva creada por {7} en fecha {8}</p></td></tr></table>",
                                    room, area, user, subject, begin, end, resources, creator, created);

                                SPUtility.SendEmail(properties.Web, headers, bodyText);
                            }
                        /*}*/
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.Continue;
                }
            }
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);

            if (properties.ListTitle == SOURCE_LIST)
            {
                try
                {
                    SPListItem theItem = properties.ListItem;
                    SPWeb theSite = properties.OpenWeb();
                    SPList theRoomList = theSite.Lists[ROOM_LIST];
                    SPList theResourceList = theSite.Lists[RESOURCE_LIST];

                    string roomNotificationMail = this.CheckForRoomNotification(theRoomList, theItem["Sala reserva"]);
                    List<string> resourceNotificationMails = this.CheckForResourceNotification(theResourceList, theItem["Recurso reserva"]);

                    #region Room notification
                    if (!string.IsNullOrWhiteSpace(roomNotificationMail))
                    {
                        string room = this.SubstringFormat(theItem["Sala reserva"]);
                        string area = this.SubstringFormat(theItem["Área reserva"]);
                        string user = theItem["Usuario reserva"].ToString();
                        string subject = theItem.Title;
                        string begin = Convert.ToDateTime(theItem["Hora de inicio"].ToString()).ToString("dd/MM/yyyy HH:mm");
                        string end = Convert.ToDateTime(theItem["Hora de finalización"].ToString()).ToString("dd/MM/yyyy HH:mm");
                        string creator = this.SubstringFormat(theItem["Creado por"]);
                        string created = Convert.ToDateTime(theItem["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm");

                        System.Collections.Specialized.StringDictionary headers =
                            new System.Collections.Specialized.StringDictionary();
                        headers.Add("to", roomNotificationMail);
                        headers.Add("cc", "");
                        headers.Add("bcc", "");
                        headers.Add("from", "reservasalas@grupobisa.com");
                        headers.Add("subject", "Notificación automática, Sistema de Reserva de Salas BISA");
                        headers.Add("content-type", "text/html");
                        string bodyText = string.Format(
                            "<table border='0' cellspacing='0' cellpadding='0' style='width:99%;border-collapse:collapse;'>" +
                            "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                            "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                            "Sistema de Reserva de Salas: Modificación reserva \"{0}\"</p></td></tr>" +
                            "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                            "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                            "Esta es una notificación automática generada por el <b>Sistema de Reserva de Salas</b> " +
                            "con el propósito de informar que se hizo la modificación de la reserva siguiente:</p>" +
                            "<p>- Sala reservada: <b>{0}</b></p>" +
                            "<p>- Área solicitante: <b>{1}</b></p>" +
                            "<p>- Usuario solicitante: <b>{2}</b></p>" +
                            "<p>- Motivo: <b>{3}</b></p>" +
                            "<p>- Periodo: desde <b>{4}</b> hasta <b>{5}</b></p>" +
                            "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                            "Reserva creada por {6} en fecha {7}</p></td></tr></table>",
                            room, area, user, subject, begin, end, creator, created);

                        SPUtility.SendEmail(properties.Web, headers, bodyText);
                    }
                    #endregion

                    #region Resource notification
                    if (resourceNotificationMails.Count > 0)
                    {
                        string room = this.SubstringFormat(theItem["Sala reserva"]);
                        string area = this.SubstringFormat(theItem["Área reserva"]);
                        string user = theItem["Usuario reserva"].ToString();
                        string subject = theItem.Title;
                        string begin = Convert.ToDateTime(theItem["Hora de inicio"].ToString()).ToString("dd/MM/yyyy HH:mm");
                        string end = Convert.ToDateTime(theItem["Hora de finalización"].ToString()).ToString("dd/MM/yyyy HH:mm");
                        string resources = this.GetFormatedResources(theItem["Recurso reserva"].ToString());
                        string creator = this.SubstringFormat(theItem["Creado por"]);
                        string created = Convert.ToDateTime(theItem["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm");

                        foreach (string resourceNotificationMail in resourceNotificationMails)
                        {
                            System.Collections.Specialized.StringDictionary headers =
                            new System.Collections.Specialized.StringDictionary();
                            headers.Add("to", resourceNotificationMail);
                            headers.Add("cc", "");
                            headers.Add("bcc", "");
                            headers.Add("from", "reservasalas@grupobisa.com");
                            headers.Add("subject", "Notificación automática, Sistema de Reserva de Salas BISA");
                            headers.Add("content-type", "text/html");
                            string bodyText = string.Format(
                                "<table border='0' cellspacing='0' cellpadding='0' style='width:99%;border-collapse:collapse;'>" +
                                "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                                "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                                "Sistema de Reserva de Salas: Modificación reserva \"{0}\"</p></td></tr>" +
                                "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                                "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                                "Esta es una notificación automática generada por el <b>Sistema de Reserva de Salas</b> " +
                                "con el propósito de informar que se hizo modificación de la reserva siguiente:</p>" +
                                "<p>- Sala reservada: <b>{0}</b></p>" +
                                "<p>- Área solicitante: <b>{1}</b></p>" +
                                "<p>- Usuario solicitante: <b>{2}</b></p>" +
                                "<p>- Motivo: <b>{3}</b></p>" +
                                "<p>- Periodo: desde <b>{4}</b> hasta <b>{5}</b></p>" +
                                "<p>- Recursos a usar: <b>{6}</b></p>" +
                                "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                                "Reserva creada por {7} en fecha {8}</p></td></tr></table>",
                                room, area, user, subject, begin, end, resources, creator, created);

                            SPUtility.SendEmail(properties.Web, headers, bodyText);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.Continue;
                }
            }
        }

        private bool ValidateNewReserve(SPItemEventProperties properties)
        {
            SPWeb theSite = properties.OpenWeb();
            SPList theList = theSite.Lists[SOURCE_LIST];

            List<CalendarEvent> newCalendarEvents = new List<CalendarEvent>();
            CalendarEvent theNewCalendarEvent = new CalendarEvent();
            theNewCalendarEvent.EventDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(properties.AfterProperties["EventDate"]));
            theNewCalendarEvent.EndDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(properties.AfterProperties["EndDate"]));
            theNewCalendarEvent.IsRecurrent = Convert.ToInt16(properties.AfterProperties["fRecurrence"]);
            theNewCalendarEvent.IsAllDayEvent = Convert.ToInt16(properties.AfterProperties["fAllDayEvent"]);
            theNewCalendarEvent.RecurrenceData = (properties.AfterProperties["RecurrenceData"] == null) ? string.Empty : properties.AfterProperties["RecurrenceData"].ToString();
            theNewCalendarEvent.EventType = properties.AfterProperties["EventType"].ToString();
            theNewCalendarEvent.RoomReserved = Convert.ToInt32(properties.AfterProperties["Sala_x0020_reserva"]);

            SPQuery query = new SPQuery();
                query.Query = string.Format("<Where><And>" +
                    "<DateRangesOverlap><FieldRef Name='EventDate' /><FieldRef Name='EndDate' /><FieldRef Name='RecurrenceID' /><Value Type='DateTime'><Now /></Value></DateRangesOverlap>" +
                    "<Eq><FieldRef Name='Sala_x0020_reserva' /><Value Type='Text'>{0}</Value></Eq>" +
                    "</And></Where>",
                    theSite.Lists[ROOM_LIST].GetItemById(theNewCalendarEvent.RoomReserved).Title);
            query.ExpandRecurrence = true;
            SPListItemCollection currentCalendarEvents = theList.GetItems(query);

            if (theNewCalendarEvent.IsRecurrent == -1)
                newCalendarEvents = RecurrenceEventsGenerator.GetAllRecurrenceEvents(theNewCalendarEvent);
            else
                newCalendarEvents.Add(theNewCalendarEvent);

            foreach (SPListItem currentCalendarEvent in currentCalendarEvents) 
	        {
                foreach (CalendarEvent newCalendarEvent in newCalendarEvents)
                {
                    if (!((newCalendarEvent.EventDate < Convert.ToDateTime(currentCalendarEvent["Hora de inicio"]) &&
                        newCalendarEvent.EndDate <= Convert.ToDateTime(currentCalendarEvent["Hora de inicio"]))
                        ||
                        (newCalendarEvent.EventDate >= Convert.ToDateTime(currentCalendarEvent["Hora de finalización"]) &&
                        newCalendarEvent.EndDate > Convert.ToDateTime(currentCalendarEvent["Hora de finalización"]))))
                    {
                        string message = string.Format(
                            "La reserva que usted trata de realizar coincide con " +
                            "<b>\"{0}\"</b> de <b>{1}</b> a <b>{2}</b> reservada por <i>{3}</i>.<br/><br/>" +
                            "Para regresar al formulario presione el botón <b>ATRAS</b> de su explorador.",
                            currentCalendarEvent.Title,
                            Convert.ToDateTime(currentCalendarEvent["Hora de inicio"]),
                            Convert.ToDateTime(currentCalendarEvent["Hora de finalización"]),
                            currentCalendarEvent["Usuario reserva"].ToString().Substring(currentCalendarEvent["Usuario reserva"].ToString().IndexOf('#') + 1));

                        properties.ErrorMessage = message;
                        properties.Status = SPEventReceiverStatus.CancelWithError;

                        return false;
                    }
                }
	        }

            return true;
        }

        private bool ValidateOldReserve(SPItemEventProperties properties)
        {
            DateTime startDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(properties.AfterProperties["EventDate"]));
            DateTime todayDate = DateTime.Now;

            if (startDate < todayDate)
            {
                properties.ErrorMessage = "No es posible hacer una reserva a una fecha u hora pasada.<br/><br/>" +
                    "Para regresar al formulario presione el botón <b>ATRAS</b> de su explorador.";
                properties.Status = SPEventReceiverStatus.CancelWithError;

                return false;
            }

            return true;
        }

        private bool ValidateChangeReserve(SPItemEventProperties properties)
        {
            if (properties.AfterProperties["Sala_x0020_reserva"] == null)
            {
                properties.ErrorMessage = "Acción no válida. Debe modificar la reserva mediante el formulario de reserva.";
                properties.Status = SPEventReceiverStatus.CancelWithError;

                return false;
            }

            if (properties.Web.CurrentUser.ID != properties.Web.Site.SystemAccount.ID &&
                !properties.Web.Groups.GetByName("Propietarios Reserva de Salas").ContainsCurrentUser &&
                properties.ListItem["Author"].ToString().Remove(properties.ListItem["Author"].ToString().IndexOf(';')) != properties.CurrentUserId.ToString())
            {
                properties.ErrorMessage = "No tiene permitido modificar una reserva realizada por otro usuario.<br/><br/>" +
                    "Para regresar al formulario presione el botón <b>ATRAS</b> de su explorador.";
                properties.Status = SPEventReceiverStatus.CancelWithError;

                return false;
            }

            int theRoom = Convert.ToInt32(properties.AfterProperties["Sala_x0020_reserva"]);
            DateTime startDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(properties.AfterProperties["EventDate"]));
            DateTime finishDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(properties.AfterProperties["EndDate"]));
            //DateTime todayDate = DateTime.Now.Date;

            SPWeb theSite = properties.OpenWeb();
            SPList theList = theSite.Lists[SOURCE_LIST];

            SPQuery query = new SPQuery();
            query.Query = string.Format("<Where><And>" +
                "<DateRangesOverlap><FieldRef Name='EventDate' /><FieldRef Name='EndDate' /><FieldRef Name='RecurrenceID' /><Value Type='DateTime'><Now /></Value></DateRangesOverlap>" +
                "<Eq><FieldRef Name='Sala_x0020_reserva' /><Value Type='Text'>{0}</Value></Eq>" +
                "</And></Where>",
                theSite.Lists[ROOM_LIST].GetItemById(theRoom).Title);
            query.ExpandRecurrence = true;
            SPListItemCollection listItems = theList.GetItems(query);

            foreach (SPListItem listItem in listItems)
            {
                if (listItem.ID != properties.ListItem.ID)
                {
                    //if (Convert.ToDateTime(listItem["Hora de finalización"]).Date >= todayDate)
                    //{
                        //if (listItem["Sala reserva"].ToString().Remove(listItem["Sala reserva"].ToString().IndexOf(';')) == theRoom)
                        //{
                            if (!((startDate < Convert.ToDateTime(listItem["Hora de inicio"]) &&
                                finishDate <= Convert.ToDateTime(listItem["Hora de inicio"]))
                                ||
                                (startDate >= Convert.ToDateTime(listItem["Hora de finalización"]) &&
                                finishDate > Convert.ToDateTime(listItem["Hora de finalización"]))))
                            {
                                string message = string.Format(
                                    "La reserva que usted trata de realizar coincide con " +
                                    "<b>\"{0}\"</b> de <b>{1}</b> a <b>{2}</b> reservada por <i>{3}</i>.<br/><br/>" +
                                    "Para regresar al formulario presione el botón <b>ATRAS</b> de su explorador.",
                                    listItem.Title,
                                    Convert.ToDateTime(listItem["Hora de inicio"]),
                                    Convert.ToDateTime(listItem["Hora de finalización"]),
                                    listItem["Usuario reserva"].ToString().Substring(listItem["Usuario reserva"].ToString().IndexOf('#') + 1));

                                properties.ErrorMessage = message;
                                properties.Status = SPEventReceiverStatus.CancelWithError;

                                return false;
                            }
                       //}
                    //}
                }
            }

            return true;
        }

        private bool ValidateDeleteReserve(SPItemEventProperties properties)
        {
            /*if (properties.AfterProperties["Sala_x0020_reserva"] == null)
            {
                properties.ErrorMessage = "Acción no válida. Debe modificar la reserva mediante el formulario de reserva.";
                properties.Status = SPEventReceiverStatus.CancelWithError;

                return false;
            }*/

            if (properties.Web.CurrentUser.ID != properties.Web.Site.SystemAccount.ID &&
                !properties.Web.Groups.GetByName("Propietarios Reserva de Salas").ContainsCurrentUser &&
                properties.ListItem["Author"].ToString().Remove(properties.ListItem["Author"].ToString().IndexOf(';')) != properties.CurrentUserId.ToString())
            {
                properties.ErrorMessage = "No tiene permitido modificar una reserva realizada por otro usuario.<br/><br/>" +
                    "Para regresar al formulario presione el botón <b>ATRAS</b> de su explorador.";
                properties.Status = SPEventReceiverStatus.CancelWithError;

                return false;
            }

            return true;
        }

        private string CheckForRoomNotification(SPList roomList, object roomItemId)
        {
            string eMail = "";

            int roomId = Convert.ToInt32(roomItemId.ToString().Remove(roomItemId.ToString().IndexOf(";")));
            SPListItem roomItem = roomList.GetItemById(roomId);

            if (roomItem["Notificar área:Correo notificación"] != null &&
                !string.IsNullOrWhiteSpace(roomItem["Notificar área:Correo notificación"].ToString()))
                eMail = this.SubstringFormat(roomItem["Notificar área:Correo notificación"]);

            return eMail;
        }

        private List<string> CheckForResourceNotification(SPList resourceList, object resourceItemId)
        {
            List<string> emails = new List<string>();

            string[] stringSeparators = new string[] { ";#" };
            string[] resourcesArray = resourceItemId.ToString().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < resourcesArray.Length; i = i + 2)
            {
                int resourceId = Convert.ToInt32(resourcesArray[i]);
                SPListItem resourceItem = resourceList.GetItemById(resourceId);

                if (resourceItem["Notificar área:Correo notificación"] != null &&
                    !string.IsNullOrWhiteSpace(resourceItem["Notificar área:Correo notificación"].ToString()))
                    emails.Add(this.SubstringFormat(resourceItem["Notificar área:Correo notificación"]));
            }

            return emails;
        }

        /// <summary>
        /// Get the formated list of resources, to display in the mail
        /// </summary>
        /// <param name="unformatedResources"></param>
        /// <returns></returns>
        private string GetFormatedResources(string unformatedResources)
        {
            string formatedResources = "";
            string[] stringSeparators = new string[] { ";#" };
            string[] resourcesArray = unformatedResources.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < resourcesArray.Length; i = i + 2)
            {
                formatedResources += resourcesArray[i + 1] + "; ";
            }

            return formatedResources;
        }

        /// <summary>
        /// Return the substring NAME of a string like '2;#NAME'
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string SubstringFormat(object value)
        {
            string formatedValue = "";

            if (value != null)
                formatedValue = value.ToString().Substring(value.ToString().IndexOf('#') + 1);

            return formatedValue;
        }
    }
}