using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace MiPagoManager.Controllers
{
    public class ExportarDataController : Controller
    {
        // GET: ExportarData
        public FileResult EscribirExcelNPOI(DataTable dt, String extencion)
        {
            IWorkbook workbook;

            if (extencion == "xlsx")
                workbook = new XSSFWorkbook();
            else if (extencion == "xls")
                workbook = new HSSFWorkbook();
            else
                throw new Exception("Este formato no es soportado..");

            ISheet sheet1 = workbook.CreateSheet("Hoja 1");

            IRow row1 = sheet1.CreateRow(0);

            for (int j = 0; j < dt.Columns.Count; j++)
            {
                ICell cell = row1.CreateCell(j);
                String columnName = dt.Columns[j].ToString();
                cell.SetCellValue(columnName);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(dt.Rows[i][columnName].ToString());
                }
            }
         
            using (var exportData = new MemoryStream())
            {
              
                workbook.Write(exportData);
                string ContentType = null;

                if (extencion == "xlsx")
                   ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                else if (extencion == "xls")
                    ContentType = "application/vnd.ms-excel";

                byte[] bytes = exportData.ToArray();
                return File(bytes, ContentType);
            }
        }
        



    }
}