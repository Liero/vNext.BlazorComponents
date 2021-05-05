using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using vNext.BlazorComponents.Demo.Shared;

namespace BlazorComponents.Demo.Tests
{
    public class ExcelExporter
    {
        private UInt32Value DateFormatStyle;
        private UInt32Value DateTimeFormatStyle;

        public int MaxColumnWidth = 50;

        public void Export<TRow>(string fileName, IEnumerable<TRow> data, IEnumerable<ColumnDefEx<TRow>> columnDefitions)
        {
            if (data is null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            if (columnDefitions is null)
            {
                throw new ArgumentNullException(nameof(columnDefitions));
            }

            CultureInfo ci = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                using (SpreadsheetDocument document = CreateSpreadsheet(fileName))
                {
                    var sheetData = GenerateSheetData(data, columnDefitions);
                    var columns = GetAutoSizedColumns(sheetData);
                    var workSheetPart = document.WorkbookPart.WorksheetParts.First();
                    workSheetPart.Worksheet.AppendChild(columns);
                    workSheetPart.Worksheet.AppendChild(sheetData);
                    document.Save();
                    document.Close();
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = ci;
            }
        }

        SpreadsheetDocument CreateSpreadsheet(string fileName)
        {
            SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
            document.AddWorkbookPart();
            document.WorkbookPart.Workbook = new Workbook();
            AddStyles(document.WorkbookPart);

            var _worksheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
            _worksheetPart.Worksheet = new Worksheet();

            Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            Sheet sheet = new Sheet()
            {
                Id = document.WorkbookPart.GetIdOfPart(_worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);
            return document;
        }

        void AddStyles(WorkbookPart workbookPart)
        {
            var dateFormat = new CellFormat
            {
                NumberFormatId = 14,
                ApplyNumberFormat = true
            };
            var dateTimeFormat = new CellFormat
            {
                NumberFormatId = 22,
                ApplyNumberFormat = true
            };
            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            var cellFormats = new List<CellFormat>
            {
                    new CellFormat(),
                    dateTimeFormat,
                    dateFormat
            };

            stylesPart.Stylesheet = new Stylesheet
            {
                Fonts = new Fonts(new Font()),
                Fills = new Fills(new Fill()),
                Borders = new Borders(new Border()),
                CellStyleFormats = new CellStyleFormats(new CellFormat()),
                CellFormats = new CellFormats(cellFormats),
            };
            DateFormatStyle = (uint)cellFormats.IndexOf(dateFormat);
            DateTimeFormatStyle = (uint)cellFormats.IndexOf(dateTimeFormat);
        }

        SheetData GenerateSheetData<TRow>(IEnumerable<TRow> data, IEnumerable<ColumnDefEx<TRow>> columnDefitions)
        {
            var sheetData = new SheetData();

            Row headerRow = new Row(columnDefitions.Select(colDef => new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(colDef.Header)
            }));
            sheetData.AppendChild(headerRow);

            foreach (var item in data)
            {
                var newRow = new Row(columnDefitions.Select(colDef => GetCell(colDef, item)));
                sheetData.AppendChild(newRow);
            };

            return sheetData;

            Cell GetCell(ColumnDefEx<TRow> colDef, TRow item)
            {
                var value = colDef.ValueGetter?.Invoke(item);
                return value switch
                {
                    null => new Cell { DataType = CellValues.String, CellValue = new CellValue("") },
                    string str => new Cell { DataType = CellValues.String, CellValue = new CellValue(str) },
                    int @int => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@int) },
                    byte @byte => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@byte) },
                    uint @uint when @uint >= 0 => new Cell { DataType = CellValues.Number, CellValue = new CellValue((int)@uint) },
                    short @short => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@short) },
                    long @long when @long <= int.MaxValue => new Cell { DataType = CellValues.Number, CellValue = new CellValue((int)@long) },
                    decimal @decimal => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@decimal) },
                    float @float => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@float) },
                    double @double => new Cell { DataType = CellValues.Number, CellValue = new CellValue(@double) },
                    DateTime datetime => new Cell
                    {
                        DataType = CellValues.Date,
                        CellValue = new CellValue(datetime),
                        StyleIndex = datetime.TimeOfDay == default ? DateFormatStyle : DateTimeFormatStyle
                    },
                    DateTimeOffset datetimeoffset => new Cell { DataType = CellValues.Date, CellValue = new CellValue(datetimeoffset) },
                    bool boolean => new Cell { DataType = CellValues.String, CellValue = new CellValue(boolean) },
                    _ => new Cell { DataType = CellValues.String, CellValue = new CellValue(Convert.ToString(value)) }
                };
            }
        }

        Columns GetAutoSizedColumns(SheetData sheetData)
        {
            //key = column index, value = char count
            var columnCharWidths = GetMaxCharacterWidth(sheetData);

            Columns columns = new Columns();
            foreach (var colCharacters in columnCharWidths)
            {
                Column col = new Column() 
                { 
                    BestFit = true, 
                    Min = (UInt32)(colCharacters.Key + 1), 
                    Max = (UInt32)(colCharacters.Key + 1),
                    CustomWidth = true,
                    Width = (DoubleValue)Math.Min(MaxColumnWidth, (colCharacters.Value * 1.08 +  1.5))
                };
                columns.Append(col);
            }

            return columns;
        }
        Dictionary<int, int> GetMaxCharacterWidth(SheetData sheetData, int rowCount = 10)
        {
            //iterate over all cells getting a max char value for each column
            Dictionary<int, int> maxColWidth = new Dictionary<int, int>();
            var rows = sheetData.Elements<Row>().Take(rowCount);
            foreach (var r in rows)
            {
                var cells = r.Elements<Cell>().ToArray();

                //using cell index as my column
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i];
                    var cellValue = cell.CellValue == null ? string.Empty : cell.CellValue.InnerText;
                    int cellTextLength = cellValue.Length;
                    if (cell.StyleIndex != null)
                    {
                        if (cell.StyleIndex == DateTimeFormatStyle)
                        {
                            cellTextLength = 15;
                        }
                        if (cell.StyleIndex == DateFormatStyle)
                        {
                            cellTextLength = 10;
                        }
                    }
                    if (maxColWidth.ContainsKey(i))
                    {
                        var current = maxColWidth[i];
                        if (cellTextLength > current)
                        {
                            maxColWidth[i] = cellTextLength;
                        }
                    }
                    else
                    {
                        maxColWidth.Add(i, cellTextLength);
                    }
                }
            }

            return maxColWidth;
        }
    }

}
