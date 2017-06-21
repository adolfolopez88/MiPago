using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiPagoManager.Models
{
    public class BuscarPagosViewModel
    {
        private Models.ApplicationDbContext db_manager = new Models.ApplicationDbContext();

        public List<string> Usuarios { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> UsuariosList
        {
            get
            {
                List<System.Web.Mvc.SelectListItem> list = null;
                if (Usuarios != null)
                    if (Usuarios.Count > 0)
                        list = db_manager.Users.Select(u => new System.Web.Mvc.SelectListItem { Selected = (Usuarios.Contains(u.Id)), Text = u.Email, Value = u.Id }).ToList();

                list = db_manager.Users.Select(u => new System.Web.Mvc.SelectListItem { Selected = true, Text = u.Email, Value = u.Id }).ToList();

                return list;
            }
        }

        [Display(Name = "Estado")]
        public string Estado { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> EstadoList
        {
            get
            {
                List<System.Web.Mvc.SelectListItem> list = new List<System.Web.Mvc.SelectListItem> {
                    new System.Web.Mvc.SelectListItem() { Text = "Todos...", Value = "TODOS", Selected=true},
                    new System.Web.Mvc.SelectListItem() { Text = "Pagados", Value = "PAGADOS" },
                    new System.Web.Mvc.SelectListItem() { Text = "Pendientes", Value = "PENDIENTES" }
                };

                return list;
            }
        }
        [Display(Name = "Fecha inicio")]
        public string FechaInicio { get; set; }
        [Display(Name = "Fecha fin")]
        public string FechaFin { get; set; }
        [Display(Name = "Orden Comercio")]
        public string AppOrdenId { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPendientes { get; set; }
        public int TotalPagados { get; set; }
        public int TotalMonto { get; set; }
        public virtual IPagedList<MiPago.Models.Pago> Pagos { get; set; }

    }
}