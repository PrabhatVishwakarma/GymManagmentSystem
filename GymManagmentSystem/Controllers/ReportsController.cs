using Microsoft.AspNetCore.Mvc;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using MongoDB.Driver;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ReportsController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/Sales?startDate=2025-01-01&endDate=2025-01-31
        [HttpGet("Sales")]
        public async Task<ActionResult<SalesReport>> GetSalesReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            // Get memberships in date range
            var filter = Builders<MembersMembership>.Filter.And(
                Builders<MembersMembership>.Filter.Gte(m => m.CreatedAt, start),
                Builders<MembersMembership>.Filter.Lte(m => m.CreatedAt, end)
            );
            var memberships = await _context.MembersMemberships.Find(filter).ToListAsync();

            // Load related enquiries and plans
            var reportItems = new List<SalesReportItem>();
            foreach (var membership in memberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                reportItems.Add(new SalesReportItem
                {
                    MembershipId = membership.MembersMembershipId,
                    MemberName = $"{enquiry?.FirstName} {enquiry?.LastName}",
                    Email = enquiry?.Email,
                    PlanName = plan?.PlanName,
                    TotalAmount = membership.TotalAmount,
                    PaidAmount = membership.PaidAmount,
                    RemainingAmount = membership.RemainingAmount,
                    Date = membership.CreatedAt
                });
            }

            var report = new SalesReport
            {
                StartDate = start,
                EndDate = end,
                TotalSales = memberships.Sum(m => m.PaidAmount),
                TotalMemberships = memberships.Count,
                AverageTicket = memberships.Any() ? memberships.Average(m => m.PaidAmount) : 0,
                PendingAmount = memberships.Sum(m => m.RemainingAmount),
                Memberships = reportItems
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

            // Get memberships in date range
            var filter = Builders<MembersMembership>.Filter.And(
                Builders<MembersMembership>.Filter.Gte(m => m.CreatedAt, start),
                Builders<MembersMembership>.Filter.Lte(m => m.CreatedAt, end)
            );
            var sort = Builders<MembersMembership>.Sort.Descending(m => m.CreatedAt);
            var memberships = await _context.MembersMemberships.Find(filter).Sort(sort).ToListAsync();

            // Load related data
            var membershipData = new List<(MembersMembership membership, Enquiry enquiry, MembershipPlan plan)>();
            foreach (var membership in memberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                membershipData.Add((membership, enquiry, plan));
            }

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
                foreach (var (membership, enquiry, plan) in membershipData)
                {
                    worksheet.Cells[row, 1].Value = membership.CreatedAt.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = $"{enquiry?.FirstName} {enquiry?.LastName}";
                    worksheet.Cells[row, 3].Value = enquiry?.Email;
                    worksheet.Cells[row, 4].Value = plan?.PlanName;
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
