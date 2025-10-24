using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/Sales?startDate=2025-01-01&endDate=2025-01-31
        [HttpGet("Sales")]
        public async Task<ActionResult<SalesReport>> GetSalesReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var memberships = await _context.MembersMemberships
                .Include(m => m.Enquiry)
                .Include(m => m.MembershipPlan)
                .Where(m => m.CreatedAt >= start && m.CreatedAt <= end)
                .ToListAsync();

            var report = new SalesReport
            {
                StartDate = start,
                EndDate = end,
                TotalSales = memberships.Sum(m => m.PaidAmount),
                TotalMemberships = memberships.Count,
                AverageTicket = memberships.Any() ? memberships.Average(m => m.PaidAmount) : 0,
                PendingAmount = memberships.Sum(m => m.RemainingAmount),
                Memberships = memberships.Select(m => new SalesReportItem
                {
                    MembershipId = m.MembersMembershipId,
                    MemberName = $"{m.Enquiry.FirstName} {m.Enquiry.LastName}",
                    Email = m.Enquiry.Email,
                    PlanName = m.MembershipPlan.PlanName,
                    TotalAmount = m.TotalAmount,
                    PaidAmount = m.PaidAmount,
                    RemainingAmount = m.RemainingAmount,
                    Date = m.CreatedAt
                }).ToList()
            };

            return Ok(report);
        }

        // GET: api/Reports/Sales/Last3Months
        [HttpGet("Sales/Last3Months")]
        public async Task<ActionResult<SalesReport>> GetLast3MonthsSales()
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-3);
            return await GetSalesReport(startDate, endDate);
        }

        // GET: api/Reports/Sales/Last6Months
        [HttpGet("Sales/Last6Months")]
        public async Task<ActionResult<SalesReport>> GetLast6MonthsSales()
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-6);
            return await GetSalesReport(startDate, endDate);
        }

        // GET: api/Reports/Sales/Monthly/2025/10
        [HttpGet("Sales/Monthly/{year}/{month}")]
        public async Task<ActionResult<SalesReport>> GetMonthlySales(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await GetSalesReport(startDate, endDate);
        }

        // GET: api/Reports/Sales/ExportToExcel?startDate=&endDate=
        [HttpGet("Sales/ExportToExcel")]
        public async Task<IActionResult> ExportSalesToExcel(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var memberships = await _context.MembersMemberships
                .Include(m => m.Enquiry)
                .Include(m => m.MembershipPlan)
                .Where(m => m.CreatedAt >= start && m.CreatedAt <= end)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Set EPPlus license context
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sales Report");

                // Add title
                worksheet.Cells[1, 1].Value = "Sales Report";
                worksheet.Cells[1, 1, 1, 8].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;

                worksheet.Cells[2, 1].Value = $"Period: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}";
                worksheet.Cells[2, 1, 2, 8].Merge = true;

                // Add headers
                int headerRow = 4;
                worksheet.Cells[headerRow, 1].Value = "Date";
                worksheet.Cells[headerRow, 2].Value = "Member Name";
                worksheet.Cells[headerRow, 3].Value = "Email";
                worksheet.Cells[headerRow, 4].Value = "Plan";
                worksheet.Cells[headerRow, 5].Value = "Total Amount";
                worksheet.Cells[headerRow, 6].Value = "Paid Amount";
                worksheet.Cells[headerRow, 7].Value = "Remaining";
                worksheet.Cells[headerRow, 8].Value = "Status";

                // Style headers
                using (var range = worksheet.Cells[headerRow, 1, headerRow, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add data
                int row = headerRow + 1;
                foreach (var membership in memberships)
                {
                    worksheet.Cells[row, 1].Value = membership.CreatedAt.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = $"{membership.Enquiry?.FirstName} {membership.Enquiry?.LastName}";
                    worksheet.Cells[row, 3].Value = membership.Enquiry?.Email;
                    worksheet.Cells[row, 4].Value = membership.MembershipPlan?.PlanName;
                    worksheet.Cells[row, 5].Value = membership.TotalAmount;
                    worksheet.Cells[row, 6].Value = membership.PaidAmount;
                    worksheet.Cells[row, 7].Value = membership.RemainingAmount;
                    worksheet.Cells[row, 8].Value = membership.RemainingAmount <= 0 ? "Fully Paid" : "Pending";
                    row++;
                }

                // Add summary
                row++;
                worksheet.Cells[row, 4].Value = "TOTAL:";
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                worksheet.Cells[row, 5].Value = memberships.Sum(m => m.TotalAmount);
                worksheet.Cells[row, 6].Value = memberships.Sum(m => m.PaidAmount);
                worksheet.Cells[row, 7].Value = memberships.Sum(m => m.RemainingAmount);

                // Format currency columns
                worksheet.Cells[headerRow + 1, 5, row, 7].Style.Numberformat.Format = "$#,##0.00";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate file
                var fileBytes = package.GetAsByteArray();
                var fileName = $"SalesReport_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }

    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalMemberships { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal PendingAmount { get; set; }
        public List<SalesReportItem> Memberships { get; set; }
    }

    public class SalesReportItem
    {
        public int MembershipId { get; set; }
        public string MemberName { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime Date { get; set; }
    }
}

