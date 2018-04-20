using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SKtimeManagement
{
    /* Sample
    var fileName = String.Format("AFL Fixture Report {0}.xls", DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss"));
    var file = String.Format(@"C:\Users\gerard.nguyen\Documents\Visual Studio 2015\Projects\AFLFixture2017Report\AFLFixture2017Report\bin\Debug\Reports\{0}", fileName);
    var workbook = new HSSFWorkbook();
    var worksheet = workbook.CreateSheet("Report");
    ExcelWorker.CellStyles = new List<ICellStyle>();
    ExcelWorker.CreateRow(worksheet, 0, new ExcelCell[] {
        ExcelWorker.CreateCell(workbook, "Date", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "Name", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "School Name", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "School Category", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "Email", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "IsAmbassador", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "Download Tipping Chart", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index),
        ExcelWorker.CreateCell(workbook, "Download Fixture Card", HSSFColor.RoyalBlue.Index, HSSFColor.White.Index)
    });
    CellRangeAddress cra = new CellRangeAddress(0, 5, 0, 0);
    worksheet.AddMergedRegion(cra);
    using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
    {
        workbook.Write(fs);
    }
    */
    public class ExcelCell
    {
        public ExcelCell(object data, ICellStyle style)
        {
            Data = data;
            Style = style;
        }
        public dynamic Data { get; set; }
        public ICellStyle Style { get; set; }
    }
    public static class ExcelWorker
    {
        public static IRow CreateRow(ISheet sheet, int index, ExcelCell[] data)
        {
            var row = sheet.CreateRow(index);

            for (var i = 0; i < data.Length; i++)
            {
                var cell = row.CreateCell(i);
                cell.SetCellValue(data[i].Data == null ? "" : data[i].Data);
                cell.CellStyle = data[i].Style;
            }

            return row;
        }
        public static ExcelCell CreateCell(HSSFWorkbook book, object data, short backColor = HSSFColor.White.Index, short fontColor = HSSFColor.Black.Index, string formatString = null)
        {
            if (ExcelWorker.CellStyles == null)
            {
                ExcelWorker.CellStyles = new List<ICellStyle>();
            }
            var style = ExcelWorker.CellStyles.Find(s => s.FillForegroundColor == backColor && s.GetFont(book) != null && s.GetFont(book).Color == fontColor);
            if (style == null)
            {
                style = book.CreateCellStyle();
                style.BorderTop = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderBottom = BorderStyle.Thin;

                style.FillForegroundColor = backColor;
                style.FillPattern = FillPattern.SolidForeground;

                IFont font = book.CreateFont();
                font.Color = fontColor;
                style.SetFont(font);

                if (!String.IsNullOrEmpty(formatString))
                {
                    var format = book.CreateDataFormat();
                    style.DataFormat = format.GetFormat(formatString);
                }

                ExcelWorker.CellStyles.Add(style);
            }
            return new ExcelCell(data, style);
        }
        public static List<ICellStyle> CellStyles { get; set; }
    }
}