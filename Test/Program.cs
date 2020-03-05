using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SharePoint;

namespace BISAUtilitary
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Utilitario eliminador de Eventos duplicados de una lista dada.");
                Console.Write("URL Sitio: ");
                string sitio = Console.ReadLine();
                Console.Write("Lista: ");
                string lista = Console.ReadLine();
                Console.WriteLine();
                Console.WriteLine("Trabajando en ello...");

                ListarEventosDeLista(sitio, lista);
                RemoverEventosDeListaDuplicados(sitio, lista);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR >> " + ex.Message);
            }
        }

        static void ListarEventosDeLista(string sitio, string lista)
        {
            //Console.WriteLine("Programa que muestra los eventos sobre una lista dada.");
            using (SPSite sps = new SPSite(sitio))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPList splist = spw.Lists[lista];
                    foreach (SPEventReceiverDefinition sprd in splist.EventReceivers)
                    {
                        Console.WriteLine(sprd.Class + " " + sprd.Name + " " + sprd.Type + " <" + sprd.Id + ">");
                    }
                }
            }

            Console.WriteLine();
        }

        static void RemoverEventosDeListaDuplicados(string sitio, string lista)
        {
            int cont = 0;

            using (SPSite sps = new SPSite(sitio))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPList spList = spw.Lists[lista];
                    List<SPEventReceiverDefinition> eventosAEliminar =
                        new List<SPEventReceiverDefinition>();

                    SPEventReceiverDefinitionCollection losEventos = spList.EventReceivers;
                    for (int i = 0; i < losEventos.Count; i++)
                    {
                        for (int j = i + 1; j < losEventos.Count; j++)
                        {
                            if (losEventos[i].Class == losEventos[j].Class &&
                                losEventos[i].Name == losEventos[j].Name &&
                                losEventos[i].Type.ToString() == losEventos[j].Type.ToString())
                            {
                                eventosAEliminar.Add(losEventos[i]);
                                break;
                            }
                        }
                    }

                    foreach (SPEventReceiverDefinition elEvento in eventosAEliminar)
                    {
                        elEvento.Delete();
                        cont++;
                    }
                }
            }

            Console.WriteLine(cont + " Eventos duplicados eliminados.");
            Console.WriteLine("Operación finalizada exitosamente.");
            Console.WriteLine();
        }
    }
}
