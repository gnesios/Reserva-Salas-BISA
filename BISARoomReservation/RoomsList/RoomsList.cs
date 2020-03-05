using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BISARoomReservation.RoomsList
{
    [ToolboxItemAttribute(false)]
    public class RoomsList : WebPart
    {
        const string ROOMS_LIST = "Salas";
        const string CALENDAR_LIST = "Reservas";

        protected override void CreateChildControls()
        {
            try
            {
                string formatedValues = this.RetrieveFormatedValuesFromList();
                LiteralControl theScript = new LiteralControl();

                if (string.IsNullOrEmpty(formatedValues))
                {
                    theScript.Text = "No existen items que mostrar.";
                }
                else
                {
                    theScript.Text =
                        "<div class='container'>" +
                        "<h1>Reserva de Salas</h1>" +
                        "<p>Elija una sala para ver el calendario con las reservas para esa sala, o presione sobre el botón <strong>RESERVAR SALA</strong> para crear una.</p>" +
                        formatedValues +
                        "</div>";
                }

                this.Controls.Add(theScript);
            }
            catch (Exception ex)
            {
                LiteralControl errorMessage = new LiteralControl();
                errorMessage.Text = "ERROR >> " + ex.Message;

                this.Controls.Clear();
                this.Controls.Add(errorMessage);
            }

        }

        private string RetrieveFormatedValuesFromList()
        {
            string formatedValues = "";

            using (SPSite sps = new SPSite(SPContext.Current.Web.Url))
            using (SPWeb spw = sps.OpenWeb())
            {
                #region Get the rooms
                SPQuery query = new SPQuery();
                query.Query = "<OrderBy><FieldRef Name='Title' Ascending='TRUE' /></OrderBy>";
                SPListItemCollection rooms = spw.Lists[ROOMS_LIST].GetItems(query);
                #endregion

                #region Get views for each room
                foreach (SPListItem room in rooms)
                {
                    try
                    {
                        SPView theView = spw.Lists[CALENDAR_LIST].Views[room.Title];
                        formatedValues += string.Format(
                            "<li data-hovercolor='#FFD300'><a href='{0}'>" +
                            "<h2 data-type='mText' class='sti-item'>{1}</h2>" +
                            "<h3 data-type='sText' class='sti-item'>{2}</h3>" +
                            "<span data-type='icon' class='sti-icon sti-item'></span>" +
                            "</a></li>",
                            theView.ServerRelativeUrl, room.Title, room["Ubicación sala"].ToString());
                    }
                    catch
                    { /*continue if the ROOM VIEW does not exist*/ }
                }
                #endregion

                formatedValues =
                    "<a class='reserve-button' href='" + sps.ServerRelativeUrl + "/Lists/Reservas/NewForm.aspx?RootFolder=Lists%2FReservas'>Reservar Sala</a>" +
                    "<ul id='sti-menu' class='sti-menu'>" + formatedValues + "</ul>";
            }

            return formatedValues;
        }
    }
}
