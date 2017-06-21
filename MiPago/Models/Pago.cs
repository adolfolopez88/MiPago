using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiPago.Models
{
    public abstract class DatosPago {
        [Required]
        public string AppEmail { get; set; }
        [Required]
        public string AppOrdenId { get; set; }
        [Required]
        public int Monto { get; set; }
        public string MiPagoOrdenId { get; set; }
        [Required]
        public DateTimeOffset FechaCreacion { get; set; }
        public DateTimeOffset? FechaPago { get; set; }
    }

    /// <summary>
    /// Pagos = Clase encargado de almacenar un pago
    /// </summary>
    [Table("Pagos")]
    public class Pago : DatosPago
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        /// <summary>
        /// Propiedad de navegacion (virtual) no es un atributo de la entidad
        /// </summary>
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual List<Log> Logs { get; set; }
        [Column(TypeName = "text")]
        public string TbkRespuesta { get; set; }
        [Required]
        public string TokenAcceso { get; set; }
        public Pago()
        {
            this.FechaCreacion = DateTime.Now;
        }

    }
    public class RequestPagoModel
    {
        /// <summary>
        /// Monto total de la transaccion.
        /// </summary>
        public string Monto { get; set; }
        /// <summary>
        /// Id de la orden de compra codigo de la aplicacion.
        /// </summary>
        public string AppOrdenId { get; set; }
    }
    public class ResponsePagoModel
    {
        public string Token { get; set; }
        public DateTimeOffset FechaCreacion { get; set; }
        public string UrlPago
        {
            get { return ConfigurationManager.AppSettings["base_url"] + "/Home/FormPago?token=" + Token; }
        }
        public ResponsePagoModel(Pago pago) {
            this.Token = pago.TokenAcceso;
            this.FechaCreacion = pago.FechaCreacion;
        }
    }
    public class ResponsePagoFinalizado : DatosPago{
        public ResponseTbk Tbk { get; set; }
        public ResponsePagoFinalizado(Pago pago) {

            this.AppEmail = pago.AppEmail;
            this.AppOrdenId = pago.AppOrdenId;
            this.MiPagoOrdenId = pago.MiPagoOrdenId;
            this.Monto = pago.Monto;
            this.FechaPago = pago.FechaPago;
            this.Tbk = new ResponseTbk(pago);
        }
    }

    /// <summary>
    /// Tbk = Toma los datos de configuracion para una solicitud de pago webpay
    /// </summary>
    public class RequestTbk
    {
        public string PathCgiBpPago { get; set; }
        public string TbkOrden { get; set; }
        public string TbkMonto { get; set; }
        public string TbkTipoTransaccion { get; set; }
        public string UrlExito { get; set; }
        public string UrlFracaso { get; set; }

        public RequestTbk(string tbk_orden, string tbk_monto) {

            this.PathCgiBpPago =  ConfigurationManager.AppSettings["base_url"] + ConfigurationManager.AppSettings["tbk_cgi_bp_pago"];
            this.TbkTipoTransaccion = ConfigurationManager.AppSettings["tbk_tipo_transaccion"];
            this.UrlExito =  ConfigurationManager.AppSettings["tbk_url_exito"];
            this.UrlFracaso = ConfigurationManager.AppSettings["tbk_url_fracaso"];  
            this.TbkOrden = tbk_orden;
            this.TbkMonto = tbk_monto + "00";
        }
    }

    /// <summary>
    /// ResponseTbk = Descomprime los datos de una respuesta de webpay
    /// </summary>
    public class ResponseTbk{

        public string TbkOrdenCompra { get; set; }
        public string TbkTipoTransaccion { get; set; }
        public string TbkRespuesta { get; set; }
        public string TbkMonto { get; set; }
        public string TbkCodigoAutorizacion { get; set; }
        public string TbkFinalNumeroTarjeta { get; set; }
        public string TbkFechaContable { get; set; }
        public string TbkFechaTransaccion { get; set; }
        public string TbkHoraTransaccion { get; set; }
        public string TbkIdTransaccion { get; set; }
        public string TbkTipoPago { get; set; }
        public string TbkNumeroCuotas { get; set; }
        public string TbkVci { get; set; }
        public string TbkMac { get; set; }
        public string TbkBaseUrl { get; set; }
        public DateTimeOffset TbkFechaMipago { get; set; }

        public ResponseTbk(Pago pago) {

            Dictionary <string,string> respuesta = new Dictionary<string, string>();
            pago.TbkRespuesta.Split('&').Select(o => o.Split('=')).ToList().ForEach(p => respuesta.Add(p[0], p[1]));

            this.TbkOrdenCompra = respuesta["TBK_ORDEN_COMPRA"];
            this.TbkTipoTransaccion = respuesta["TBK_TIPO_TRANSACCION"];
            this.TbkRespuesta = respuesta["TBK_RESPUESTA"];
            this.TbkMonto = pago.Monto.ToString();
            this.TbkCodigoAutorizacion = respuesta["TBK_CODIGO_AUTORIZACION"];
            this.TbkFinalNumeroTarjeta = respuesta["TBK_FINAL_NUMERO_TARJETA"];
            this.TbkFechaContable = respuesta["TBK_FECHA_CONTABLE"];
            this.TbkFechaTransaccion = respuesta["TBK_FECHA_TRANSACCION"];
            this.TbkHoraTransaccion = respuesta["TBK_HORA_TRANSACCION"];
            this.TbkIdTransaccion = respuesta["TBK_ID_TRANSACCION"];
            this.TbkTipoPago = respuesta["TBK_TIPO_PAGO"];
            this.TbkNumeroCuotas = respuesta["TBK_NUMERO_CUOTAS"];
            this.TbkVci = respuesta["TBK_VCI"];
            this.TbkMac = respuesta["TBK_MAC"];
            this.TbkBaseUrl = ConfigurationManager.AppSettings["base_url"];
            this.TbkFechaMipago = pago.FechaPago.Value;
        }

    }
}