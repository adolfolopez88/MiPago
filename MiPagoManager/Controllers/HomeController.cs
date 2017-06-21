using MiPago.Models;
using MiPagoManager.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace MiPagoManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private MiPago.Models.ApplicationDbContext db = new MiPago.Models.ApplicationDbContext();
        private Models.ApplicationDbContext db_manager = new Models.ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(BuscarPagosViewModel model, int? page, int excel = 0)
        {
            try
            {
                if (Session["FiltrosBuscarPagos"] != null)
                    model = (BuscarPagosViewModel)Session["FiltrosBuscarPagos"];
                else
                    model.Usuarios = new List<string>();

                DateTimeOffset fecha_inicio = Convert.ToDateTime(model.FechaInicio);
                DateTimeOffset fecha_fin = Convert.ToDateTime(model.FechaFin);

                if (string.IsNullOrEmpty(model.FechaFin) && string.IsNullOrEmpty(model.FechaFin))
                {
                    fecha_inicio = DateTimeOffset.Now.AddDays(-30);
                    fecha_fin = DateTimeOffset.Now;
                    model.FechaInicio = fecha_inicio.ToString("dd/MM/yyyy");
                    model.FechaFin = fecha_fin.ToString("dd/MM/yyyy");
                }

                IQueryable<Pago> pagos = null;
                if (UserManager.IsInRole(User.Identity.GetUserId(), "Administrador"))
                    pagos = db.Pagos.Include("ApplicationUser").AsQueryable();
                else
                {
                    string user_id = User.Identity.GetUserId();
                    pagos = db.Pagos.Where(p => p.UserId == user_id).Include("ApplicationUser").AsQueryable();
                }

                if (model.Usuarios != null)
                    if (model.Usuarios.Count > 0)
                        pagos = pagos.Where(p => model.Usuarios.Contains(p.ApplicationUser.Id)).AsQueryable();

                if (!string.IsNullOrEmpty(model.AppOrdenId))
                    pagos = pagos.Where(p => p.AppOrdenId == model.AppOrdenId).AsQueryable();

                if (!string.IsNullOrEmpty(model.FechaInicio) && !string.IsNullOrEmpty(model.FechaFin))
                    pagos = pagos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) >= DbFunctions.TruncateTime(fecha_inicio) &&
                    DbFunctions.TruncateTime(p.FechaCreacion) <= DbFunctions.TruncateTime(fecha_fin)).AsQueryable();
                else if (!string.IsNullOrEmpty(model.FechaInicio))
                    pagos = pagos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) >= DbFunctions.TruncateTime(fecha_inicio)).AsQueryable();
                else if (!string.IsNullOrEmpty(model.FechaFin))
                    pagos = pagos.Where(p => DbFunctions.TruncateTime(p.FechaCreacion) <= DbFunctions.TruncateTime(fecha_fin)).AsQueryable();

                if(model.Estado == "PAGADOS")
                    pagos = pagos.Where(p => p.FechaPago != null).AsQueryable();
                else if (model.Estado == "PENDIENTES")
                    pagos = pagos.Where(p => p.FechaPago == null).AsQueryable();

                model.TotalRegistros = Convert.ToInt32(pagos.Count());
                model.TotalPagados = Convert.ToInt32(pagos.Where(p => p.FechaPago != null).Count());
                model.TotalPendientes = Convert.ToInt32(pagos.Where(p => p.FechaPago == null).Count());

                if (model.TotalRegistros > 0)
                    model.TotalMonto = Convert.ToInt32(pagos.Sum(p => p.Monto));

                int pageSize = 10;
                int pageNumber = (page ?? 1);

                if (excel == 1)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Cliente", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));
                    dt.Columns.Add("Estado", typeof(string));
                    dt.Columns.Add("Monto",  typeof(int));

                    foreach (var item in pagos)
                        dt.Rows.Add(item.AppEmail, item.FechaCreacion, (!string.IsNullOrEmpty(item.FechaPago.ToString())) ? "Pagado" : "Pendiente",  item.Monto);

                    ExportarDataController exportar = new ExportarDataController();
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", $"pagos_{DateTime.Now.Millisecond.ToString()}.xls"));
                    return exportar.EscribirExcelNPOI(dt, "xls");
                }     
                  
                model.Pagos = pagos.OrderByDescending(p => p.FechaCreacion).ToPagedList(pageNumber, pageSize);
                return View(model);
               
            }
            catch (Exception ex){

                return View();
            }   
        }

        [HttpPost]
        public ActionResult FiltrosBuscarPagos(BuscarPagosViewModel model) {
            Session["FiltrosBuscarPagos"] = model;

            return Redirect("/Home/");
        }

        [HttpGet]
        public ActionResult PagoLogs(int pago_id) {

            List<Log> logs = db.Pagos.First(p => p.Id == pago_id).Logs
                .Select(l => new Log {  Id = l.Id, Modulo = l.Modulo, FechaCreacion = l.FechaCreacion, DetalleDesencriptado = Log.DesncriptarDetalle(l.Detalle)}).ToList();
        
            return PartialView(logs);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}