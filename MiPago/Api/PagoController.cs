using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MiPago.Models;
using System.Configuration;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MiPago.Api
{
    public class PagoController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
   
        /// <summary>
        /// Retorna una lista de pagos finalizados con exito.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrador")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IQueryable<Pago> GetPagos()
        {
            string app_id = this.RequestContext.Principal.Identity.Name;
            return db.Pagos.Where((p) => p.AppEmail == app_id && p.FechaPago != null);
        }

        // POST: api/PagoService
        /// <summary>
        /// Registra o retorna una solicitud de pago nueva o ya existente.
        /// </summary>
        /// <param name="pago_view"></param>
        /// <returns></returns>
        [Authorize]
        [ResponseType(typeof(Pago))]
        public async Task<IHttpActionResult> PostPago(RequestPagoModel pago_view)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string user_id = User.Identity.GetUserId();
            int monto = Convert.ToInt32(pago_view.Monto);
            Pago pago = await db.Pagos.FirstOrDefaultAsync((p) => p.UserId == user_id && p.AppOrdenId == pago_view.AppOrdenId && p.Monto == monto);
            string dec_token = "app_id=" + user_id + "&app_orden_id=" + pago_view.AppOrdenId;

            if (pago == null)
            {
                pago = new Pago();
                string enc_token = HttpServerUtility.UrlTokenEncode(Convert.FromBase64String(EncryptDecrypt.EncryptText(dec_token, ConfigurationManager.AppSettings["encrypt_pass"])));
                pago.AppEmail = User.Identity.GetUserName();
                pago.AppOrdenId = pago_view.AppOrdenId;
                pago.TokenAcceso = enc_token;
                pago.Monto = monto;
                pago.UserId = user_id;

                db.Pagos.Add(pago);
                await db.SaveChangesAsync();

                Log.CreateLog("API-solicitud_de_pago_nueva", dec_token+ "&monto="+ monto.ToString(), pago.Id);
            }
            else if(pago.FechaPago == null)
                Log.CreateLog("API-solicitud_de_pago_existente", dec_token + "&monto=" + monto.ToString(), pago.Id);

            if (pago.FechaPago != null)
            {
                ResponsePagoFinalizado response_pago_finalizado = new ResponsePagoFinalizado(pago);
                Log.CreateLog("API-solicitud_de_pago_finalizado", dec_token + "&monto=" + monto.ToString(), pago.Id);
                return CreatedAtRoute("DefaultApi", new { id = pago.TokenAcceso }, response_pago_finalizado);
            }
            
            ResponsePagoModel response_pago = new ResponsePagoModel(pago);
            return CreatedAtRoute("DefaultApi", new { id = pago.TokenAcceso }, response_pago);
        }

        // DELETE: api/PagoService/5
        [ResponseType(typeof(Pago))]
        [Authorize(Roles = "Administrador")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IHttpActionResult> DeletePago(int id)
        {
            Pago pago = await db.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }

            db.Pagos.Remove(pago);
            await db.SaveChangesAsync();

            return Ok(pago);
        }

        // PUT: api/PagoService/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Administrador")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IHttpActionResult> PutPago(int id, Pago pago)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pago.Id)
            {
                return BadRequest();
            }

            db.Entry(pago).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PagoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PagoExists(int id)
        {
            return db.Pagos.Count(e => e.Id == id) > 0;
        }
    }
}