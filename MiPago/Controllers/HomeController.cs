using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiPago.Models;
using System.Configuration;
using System.IO;
using System.Data.Entity;

namespace MiPago.Controllers
{
    //[Authorize(Roles="admin")]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //permite entrar al metodo sin autenticarse
        //[AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
        
            if(ConfigurationManager.AppSettings["ip_habilitada"] == Request.UserHostAddress)
                return View("PruebaApi");

            return View();
        }

        public ActionResult PruebaApiLogin() {

            ViewBag.Title = "Home Page";
            //if (ConfigurationManager.AppSettings["ip_habilitada"] == Request.UserHostAddress)
              //  return View("PruebaApiLogin");

            return View("PruebaApiLogin");
        }

        public ActionResult FormPago()
        {
            byte[] token = HttpServerUtility.UrlTokenDecode(Convert.ToString(Request.QueryString["token"]));
            string token_data = EncryptDecrypt.DecryptText(token, ConfigurationManager.AppSettings["encrypt_pass"]);

            string app_id = HttpUtility.ParseQueryString(token_data).Get("app_id");
            string app_orden_id = HttpUtility.ParseQueryString(token_data).Get("app_orden_id");

            Pago pago = db.Pagos.FirstOrDefault((p) => p.UserId == app_id && p.AppOrdenId == app_orden_id);

            if (pago != null)
            {
                if (pago.MiPagoOrdenId == null)
                {
                    pago.MiPagoOrdenId = GenerarIdOrden();
                    db.Entry(pago).State = EntityState.Modified;
                    db.SaveChanges();
                }

                ViewBag.tbk = new RequestTbk(pago.MiPagoOrdenId, pago.Monto.ToString());
                return View("redireccionar");
            }
        
            return HttpNotFound();
        }

        public ActionResult XtCompra()
        {
            try
            {
                string tbk_respuesta = Request.Form["TBK_RESPUESTA"];

                if (tbk_respuesta == "0")
                {
                    string tbk_orden_compra = Request.Form["TBK_ORDEN_COMPRA"];
                    Pago pago = db.Pagos.FirstOrDefault((p) => p.MiPagoOrdenId == tbk_orden_compra);

                    bool validar_pago = (pago != null);
                    bool validar_monto = ((pago.Monto.ToString() + "00") == Request.Form["TBK_MONTO"]);
                    bool validar_finalizado = (pago.FechaPago == null);
                    bool validar_mac = (CheckMac() == "CORRECTO");

                    string log_detalle = $"pago_existe={validar_pago.ToString()}&montos_iguales={validar_monto.ToString()}&pago_no_finalizado={validar_finalizado.ToString()}&mac_valida={validar_mac.ToString()}";
                    Log.CreateLog($"XTCOMPRA-validacion_{tbk_orden_compra}", log_detalle, pago.Id);

                    if (!validar_pago || !validar_monto || !validar_finalizado || !validar_mac)
                        return Content("RECHAZADO");
                    
                    pago.TbkRespuesta = Convert.ToString(Request.Form);
                    pago.FechaPago = DateTimeOffset.Now; 
                    db.Entry(pago).State = EntityState.Modified;

                    Log.CreateLog($"XTCOMPRA-exito_{tbk_orden_compra}", Convert.ToString(Request.Form), pago.Id);
                    db.SaveChanges();
                }

                return Content("ACEPTADO");
            }
            catch (Exception ex)
            {
                return Content("RECHAZADO");
            }
        }

        public ActionResult Exito()
        {
            if (!string.IsNullOrEmpty(Request.Form["TBK_ORDEN_COMPRA"]))
            {
                string tbk_orden_compra = Request.Form["TBK_ORDEN_COMPRA"];
                Pago pago = db.Pagos.FirstOrDefault((p) => p.MiPagoOrdenId == tbk_orden_compra);

                if (pago == null)
                    return HttpNotFound();

                ResponseTbk tbk_respuesta = new ResponseTbk(pago);
                ViewBag.tbk_respuesta = tbk_respuesta;

                return View("Exito");
            }

            return HttpNotFound();
        }

        /// <summary>
        /// Muestra un mensaje de fracaso en caso de que exista algun error en la transacción.
        /// </summary>
        /// <returns></returns>
        public ActionResult Fracaso()
        {
            if (!string.IsNullOrEmpty(Request.Form["TBK_ORDEN_COMPRA"]))
            {
                string tbk_orden_compra = Convert.ToString(Request.Form["TBK_ORDEN_COMPRA"]);
                Pago pago = db.Pagos.FirstOrDefault((p) => p.MiPagoOrdenId == tbk_orden_compra);

                if(pago == null)
                    return Redirect("/Home/");

                ViewBag.tbk_orden_compra = Request.Form["TBK_ORDEN_COMPRA"];
                ViewBag.pago_url = ConfigurationManager.AppSettings["base_url"] + "/Home/FormPago?token=" + pago.TokenAcceso;

                return View("Fracaso");
            }
            else
                return Redirect("/Home/");
        }

        private string CheckMac()
        {
            try
            {
                string ruta_carpeta_log = Server.MapPath("~/webpay/cgi-bin/log/");
                string ruta_archivo_bat = Server.MapPath("~/webpay/cgi-bin/tbk_check_mac.bat");
                string ruta_ejecutable_mac = Server.MapPath("~/webpay/cgi-bin/tbk_check_mac.exe");

                string archivo_temporal = ruta_carpeta_log + "DatosParaCheckMac_" + Request.Form["TBK_ORDEN_COMPRA"] + ".txt";
                string archivo_resultado = ruta_carpeta_log + "ResultadoCheckMac_" + Request.Form["TBK_ORDEN_COMPRA"] + ".txt";

                StreamWriter file = new StreamWriter(archivo_temporal);
                file.WriteLine(Request.Form);
                file.Close();

                string cmd = ruta_archivo_bat + " " + ruta_ejecutable_mac + " " + archivo_temporal + " " + archivo_resultado;

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                shell.run(cmd, 0, true);

                string resultado = System.IO.File.ReadAllText(archivo_resultado);
                return resultado.Replace("\r\n", "");
            }
            catch (Exception ex)
            {
                return "INVALIDO";
            }
        }

        private string GenerarIdOrden() {

            DateTime dia_actual = DateTime.Now;

            string id_orden = dia_actual.Day.ToString() + dia_actual.Month + dia_actual.Year.ToString();
            id_orden += dia_actual.Hour.ToString() + dia_actual.Minute.ToString() + dia_actual.Second.ToString() + dia_actual.Millisecond.ToString();

            return id_orden;
        }
    }
}
