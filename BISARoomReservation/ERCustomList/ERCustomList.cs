using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace BISARoomReservation.ERCustomList
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ERCustomList : SPItemEventReceiver
    {
        const string CALENDAR_LIST = "Reservas";
        const string SOURCE_LIST = "Salas";

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
                    SPListItem theItem = properties.ListItem;

                    if (theItem.Title != properties.AfterProperties["Title"].ToString())
                    {
                        SPWeb theSite = properties.OpenWeb();
                        SPList theList = theSite.Lists[CALENDAR_LIST];
                        SPViewCollection theViews = theList.Views;

                        try { this.DeleteTheView(theViews, theItem.Title); } catch { }

                        this.CreateTheView(theViews, properties.AfterProperties["Title"].ToString());
                    }
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
                    SPListItem theItem = properties.ListItem;
                    SPWeb theSite = properties.OpenWeb();
                    SPList theList = theSite.Lists[CALENDAR_LIST];
                    SPViewCollection theViews = theList.Views;

                    this.DeleteTheView(theViews, theItem.Title);
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.Continue;
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
                    SPList theList = theSite.Lists[CALENDAR_LIST];
                    SPViewCollection theViews = theList.Views;

                    this.CreateTheView(theViews, theItem.Title);
                }
                catch (Exception ex)
                {
                    properties.ErrorMessage = ex.Message;
                    properties.Status = SPEventReceiverStatus.CancelWithError;
                }
            }
        }

        private void CreateTheView(SPViewCollection theViews, string viewName)
        {
            try
            {//if the view exist do nothing
                SPView theView = theViews[viewName];
            }
            catch
            {//if not, create it
                System.Collections.Specialized.StringCollection viewFields =
                    new System.Collections.Specialized.StringCollection();

                viewFields.Add("EventDate");
                viewFields.Add("EndDate");
                viewFields.Add("Title");
                //viewFields.Add("_x00c1_rea_x0020_reserva");
                viewFields.Add("fRecurrence");

                string query = string.Format(
                    "<Where>" +
                    "<And>" +
                    "<DateRangesOverlap>" +
                    "<FieldRef Name='EventDate' />" +
                    "<FieldRef Name='EndDate' />" +
                    "<FieldRef Name='RecurrenceID' />" +
                    "<Value Type='DateTime'><Month /></Value>" +
                    "</DateRangesOverlap>" +
                    "<Eq><FieldRef Name='Sala_x0020_reserva' /><Value Type='Text'>{0}</Value></Eq>" +
                    "</And>" +
                    "</Where>", viewName);

                SPView theView = theViews.Add(viewName, viewFields, query, 0, false, false, SPViewCollection.SPViewType.Calendar | SPViewCollection.SPViewType.Recurrence, false);
                theView.ViewData =
                    "<FieldRef Name='Title' Type='CalendarMonthTitle' />" +
                    "<FieldRef Name='Title' Type='CalendarWeekTitle' />" +
                    "<FieldRef Name='_x00c1_rea_x0020_reserva' Type='CalendarWeekLocation' />" +
                    "<FieldRef Name='Title' Type='CalendarDayTitle' />" +
                    "<FieldRef Name='_x00c1_rea_x0020_reserva' Type='CalendarDayLocation' />";
                theView.Scope = SPViewScope.Recursive;
                theView.MobileView = true;
                theView.Update();
            }
        }

        private void DeleteTheView(SPViewCollection theViews, string viewName)
        {
            theViews.Delete(theViews[viewName].ID);
        }
    }
}