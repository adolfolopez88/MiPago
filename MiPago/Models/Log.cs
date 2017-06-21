using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiPago.Models
{

    public class Log
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTimeOffset FechaCreacion { get; set; }
        [Required]
        public string Modulo { get; set; }
        [Required]
        public string Detalle { get; set; }

        [ForeignKey("Pago")]
        public int? PagoId { get; set; }
        public virtual Pago Pago { get; set; }
        public virtual Dictionary<string, string> DetalleDesencriptado { get; set; }
        public static void CreateLog(string modulo, string detalle, int? pago_id) {

            ApplicationDbContext db = new ApplicationDbContext();

            Log log = new Log();
            log.Modulo = modulo;
            log.Detalle = detalle;
            log.FechaCreacion = DateTimeOffset.Now;
            log.PagoId = pago_id;

            db.Log.Add(log);
            db.SaveChanges();
        }
        public static Dictionary<string, string> DesncriptarDetalle(string detalle){

            Dictionary<string, string> respuesta = new Dictionary<string, string>();
            detalle.Split('&').Select(o => o.Split('=')).ToList().ForEach(p => respuesta.Add(p[0], p[1]));
            return respuesta;
        }
    }
}