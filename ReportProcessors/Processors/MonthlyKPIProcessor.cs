﻿using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    internal class MonthlyKPIProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        /// <summary>
        /// Формирует отчёт KPI для отдела розницы. Собирает показатели по каждому менеджеру по итогам месяца.
        /// </summary>
        internal MonthlyKPIProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
        }

        private readonly List<(int, string)> managers = new()
        {
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (2976226, "Вера Гладкова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (6769426, "Рюмина Наталья"),
            (2375152, "Карен Оганисян"),
            (3813670, "Федорова Александра"),
            (6102562, "Валерия Лукьянова"),
            (6410290, "Вероника Бармина"),
            (6699043, "Татьяна Ганоу"),
            (6729241, "Серик Айбасов"),
        };

        private readonly List<(int, int)> dataRanges = new()
        {
            (1601499600, 1604177999),    //октябрь
            (1604178000, 1606769999),    //ноябрь
            (1606770000, 1609448399),    //декабрь
            (1609448400, 1612126799),    //январь
            (1612126800, 1614545999)     //февраль
        };

        private readonly List<int> pipelines = new()
        {
            3198184,
            3566374,
            3558964,
            3558991,
            3558922
        };

        private readonly Dictionary<string, CellFormat> columns = new()
        {
            { "A", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "B", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "C", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "D", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "E", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "F", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "G", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "H", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "I", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ### сек" } } },
            { "J", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "K", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "L", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "M", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } }
        };
        #endregion

        #region Supplementary methods
        private static List<Request> GetHeaderRequests(int? sheetId)
        {
            List<Request> requestContainer = new();

            #region Creating CellFormat for header
            var centerAlignment = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 11
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            #region Adding header
            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = sheetId },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = ""} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Закрытых сделок"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "из них очные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "дистанционные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "повторные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Кол-во исходящих"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "первых звонков"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "прочих"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Длительность исходящих, сек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Отправлено КП"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Переведено из акутализации"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Акций"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Новых продуктов"} }
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 168, 120, 84, 72, 108, 96, 120, 108, 144, 120, 108, 108, 108 };
            int i = 0;

            foreach (var c in width)
            {
                requestContainer.Add(new Request()
                {
                    UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                    {
                        Fields = "PixelSize",
                        Range = new DimensionRange() { SheetId = sheetId, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                        Properties = new DimensionProperties() { PixelSize = c }
                    }
                });
                i++;
            }
            #endregion

            return requestContainer;
        }

        private CellData[] GetCellData(string A, int B, int C, int D, int E, int F, int G, int I, int J, int K, int L, int M)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = A},
                    UserEnteredFormat = columns["A"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = B},
                    UserEnteredFormat = columns["B"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = C},
                    UserEnteredFormat = columns["C"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = D},
                    UserEnteredFormat = columns["D"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = E},
                    UserEnteredFormat = columns["E"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = F},
                    UserEnteredFormat = columns["F"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = G},
                    UserEnteredFormat = columns["G"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-2]"", FALSE)-INDIRECT(""R[0]C[-1]"", FALSE)"},
                    UserEnteredFormat = columns["H"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = I},
                    UserEnteredFormat = columns["I"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = J},
                    UserEnteredFormat = columns["J"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = K},
                    UserEnteredFormat = columns["K"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = L},
                    UserEnteredFormat = columns["L"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = M},
                    UserEnteredFormat = columns["M"] },
            };
        }

        private async Task PrepareSheets()
        {
            List<Request> requestContainer = new();

            #region Retrieving spreadsheet
            var spreadsheet = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            #endregion

            #region Deleting existing sheets except first
            foreach (var s in spreadsheet.Sheets)
            {
                if (s.Properties.SheetId == 0) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
            #endregion

            foreach (var m in managers)
            {
                #region Adding sheet
                requestContainer.Add(new Request()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = columns.Count,
                                FrozenRowCount = 1
                            },
                            Title = m.Item2,
                            SheetId = m.Item1
                        }
                    }
                });
                #endregion

                requestContainer.AddRange(GetHeaderRequests(m.Item1));
            }

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private async Task ProcessManager((int, string) manager, (int, int) dataRange)
        {
            //Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";

            //Список новых сделок в воронках из pipelines
            _processQueue.AddSubTask(_taskName, $"{_taskName}_{manager.Item2}", $"KPIReport: {dates}, new leads");

            List<Lead> newLeads = new();

            Parallel.ForEach(pipelines, p => {
                var range = _leadRepo.GetByCriteria($"filter[pipeline_id][0]={p}&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}&with=contacts");
                lock (newLeads)
                {
                    newLeads.AddRange(range);
                }
            });

            int totalNewLeads = newLeads.Count;

            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"KPIReport: {dates}, new leads: {totalNewLeads}");

            double responseTime = GetAverageResponseTime(newLeads, _longAnsweredLeads, _leadRepo, _contRepo);
            int longLeads = _longAnsweredLeads.Count(x => x.Item1 == manager.Item1);

            //Список закрытых сделок
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"KPIReport: {dates}, closed leads");

            var allLeads = _leadRepo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={dataRange.Item1}&filter[closed_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}");

            //Количество закрытых сделок
            int finishedLeads = allLeads.Where(x => (x.status_id == 142) || (x.status_id == 143)).Count();

            //Количество успешных сделок
            int successLeads = allLeads.Where(x => x.status_id == 142).Count();

            //Список звонков
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, getting calls");
            Calls calls = new(dataRange, _contRepo, manager.Item1);

            //Количество исходящих вызовов
            int outCallsCount = calls.outCalls.Count();

            //Количество входящих вызовов
            int inCallsCount = calls.inCalls.Count();

            //Количество пропущенных вызовов
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"KPIReport: {dates}, missed calls");

            int missedCallsCount = 0;

            var callIdList = new List<int>();

            foreach (var e in calls.inCalls)
                callIdList.Add(e.value_after[0].note.id);

            foreach (var n in _contRepo.BulkGetNotesById(callIdList))
            {
                int duration = -1;

                if (n.parameters is not null)
                    duration = (int)n.parameters.duration;

                if (duration == 0) missedCallsCount++;
            }

            //Всего продаж
            int totalSales = allLeads.Where(x => x.status_id == 142).Sum(n => (int)n.price);

            //Время сделки
            double averageTime = 0;
            if (finishedLeads > 0)
                averageTime = allLeads.AsParallel()
                    .Where(x => (x.status_id == 142) || (x.status_id == 143))
                    .Select(x => (int)x.closed_at - (int)x.created_at).Average() / 86400;

            List<Request> requestContainer = new();
            requestContainer.Add(GetRowRequest(manager.Item1, GetCellData(dates, totalNewLeads, finishedLeads, successLeads, totalSales, 0, (int)averageTime, (int)responseTime, longLeads, inCallsCount, outCallsCount, missedCallsCount)));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove($"{_taskName}_{manager.Item2}");
        }

        private async Task FinalizeManagers()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare Data
                List<(int?, int, int, int?)> leads = new();
                if (_longAnsweredLeads.Any(x => x.Item1 == m.Item1))
                    leads.AddRange(_longAnsweredLeads.Where(x => x.Item1 == m.Item1));
                var rows = new List<RowData>();

                #region Header
                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Сделка" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа, сек" } }
                        }
                });
                #endregion

                foreach (var l in leads)
                {
                    rows.Add(new RowData()
                    {
                        Values = new List<CellData>(){
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{l.Item2}"", ""{l.Item2}"")" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{l.Item3}" } }
                        }
                    });
                }
                #endregion

                #region Add Request
                requestContainer.Add(new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Fields = '*',
                        Rows = rows,
                        Range = new GridRange()
                        {
                            SheetId = m.Item1,
                            StartRowIndex = dataRanges.Count + 3,
                            EndRowIndex = dataRanges.Count + 3 + rows.Count,
                            StartColumnIndex = 0,
                            EndColumnIndex = 2
                        }
                    }
                });
                #endregion

                #region Add banding
                requestContainer.Add(new Request()
                {
                    AddBanding = new AddBandingRequest()
                    {
                        BandedRange = new BandedRange()
                        {
                            Range = new GridRange() { SheetId = m.Item1, StartRowIndex = 1, EndRowIndex = dataRanges.Count + 1 },
                            RowProperties = new BandingProperties()
                            {
                                FirstBandColor = new Color() { Red = 217f / 255, Green = 234f / 255, Blue = 211f / 255 },
                                SecondBandColor = new Color() { Red = 182f / 255, Green = 215f / 255, Blue = 168f / 255 },
                            }
                        }
                    }
                });
                #endregion
            }

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private async Task FinalizeTotals()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare data
                var rows = new List<RowData>
                {
                    new RowData()
                    {
                        Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = $"{m.Item2}"},
                             UserEnteredFormat = columns["A"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!B{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["B"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!C{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["C"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!D{dataRanges.Count + 2}"},
                             UserEnteredFormat = columns["D"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!E{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["E"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!F{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["F"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!G{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["G"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!H{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["H"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!I{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["I"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!J{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["J"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!K{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["K"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!L{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["L"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!M{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["M"] },
                }
                    }
                };
                #endregion

                #region Add request
                requestContainer.Add(new Request()
                {
                    AppendCells = new AppendCellsRequest()
                    {
                        Fields = '*',
                        Rows = rows,
                        SheetId = 0
                    }
                });
                #endregion            
            }

            #region Add banding
            requestContainer.Add(new Request()
            {
                AddBanding = new AddBandingRequest()
                {
                    BandedRange = new BandedRange()
                    {
                        Range = new GridRange() { SheetId = 0, StartRowIndex = 1, EndRowIndex = managers.Count + 1 },
                        BandedRangeId = 0,
                        RowProperties = new BandingProperties()
                        {
                            FirstBandColor = new Color() { Red = 217f / 255, Green = 234f / 255, Blue = 211f / 255 },
                            SecondBandColor = new Color() { Red = 182f / 255, Green = 215f / 255, Blue = 168f / 255 },
                        }
                    }
                }
            });
            #endregion

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }
        #endregion

        #region Realization
        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskName);
                return;
            }

            await PrepareSheets();

            foreach (var d in dataRanges)
            {
                if (_token.IsCancellationRequested) break;
                _longAnsweredLeads = new();
                List<Task> tasks = new();

                foreach (var manager in managers)
                {
                    if (_token.IsCancellationRequested) break;
                    var m = manager;
                    tasks.Add(Task.Run(() => ProcessManager(m, d), _token));
                }

                await Task.WhenAll(tasks);
            }

            await FinalizeManagers();
            await FinalizeTotals();

            _processQueue.Remove(_taskName);
        }
        #endregion
    }
}