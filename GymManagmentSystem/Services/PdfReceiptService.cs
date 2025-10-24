using System.Text;
using GymManagmentSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GymManagmentSystem.Services
{
    public class PdfReceiptService
    {
        public byte[] GenerateReceiptPdf(PaymentReceipt receipt)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("GYM MANAGEMENT SYSTEM")
                                .FontSize(24).Bold().FontColor(Colors.White);
                            column.Item().Text("Payment Receipt")
                                .FontSize(16).FontColor(Colors.White);
                        });

                    page.Content()
                        .Padding(20)
                        .Column(column =>
                        {
                            // Receipt Number and Date
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Receipt #: {receipt.ReceiptNumber}").Bold().FontSize(14);
                                    col.Item().Text($"Date: {receipt.PaymentDate:dd MMM yyyy}").FontSize(10);
                                });
                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text($"Received By: {receipt.ReceivedBy}").FontSize(10);
                                });
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Member Information
                            column.Item().PaddingTop(15).Text("Member Information").Bold().FontSize(12);
                            column.Item().PaddingTop(5).Column(col =>
                            {
                                col.Item().Text($"Name: {receipt.MemberName}");
                                col.Item().Text($"Email: {receipt.MemberEmail}");
                                col.Item().Text($"Phone: {receipt.MemberPhone}");
                                col.Item().Text($"Plan: {receipt.PlanName}");
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            // Payment Details
                            column.Item().PaddingTop(15).Text("Payment Details").Bold().FontSize(12);
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Description").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Amount").Bold();
                                });

                                // Rows
                                table.Cell().Padding(5).Text("Total Membership Amount");
                                table.Cell().Padding(5).AlignRight().Text($"₹{receipt.TotalAmount:N2}");

                                table.Cell().Padding(5).Text("Previous Payments");
                                table.Cell().Padding(5).AlignRight().Text($"₹{receipt.PreviousPaid:N2}");

                                table.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("Current Payment").Bold();
                                table.Cell().Background(Colors.Green.Lighten4).Padding(5).AlignRight().Text($"₹{receipt.AmountPaid:N2}").Bold().FontSize(12);

                                table.Cell().Padding(5).Text("Total Paid");
                                table.Cell().Padding(5).AlignRight().Text($"₹{(receipt.PreviousPaid + receipt.AmountPaid):N2}");

                                table.Cell().Background(Colors.Orange.Lighten4).Padding(5).Text("Remaining Balance").Bold();
                                table.Cell().Background(Colors.Orange.Lighten4).Padding(5).AlignRight().Text($"₹{receipt.RemainingAmount:N2}").Bold();
                            });

                            // Payment Method
                            column.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Text($"Payment Method: {receipt.PaymentMethod}");
                                if (!string.IsNullOrEmpty(receipt.TransactionId))
                                {
                                    row.RelativeItem().Text($"Transaction ID: {receipt.TransactionId}");
                                }
                            });

                            // Notes
                            if (!string.IsNullOrEmpty(receipt.Notes))
                            {
                                column.Item().PaddingTop(10).Text($"Notes: {receipt.Notes}").Italic();
                            }

                            // Thank you message
                            column.Item().PaddingTop(30).AlignCenter().Text("Thank you for your payment!")
                                .FontSize(12).Italic().FontColor(Colors.Blue.Medium);
                        });

                    page.Footer()
                        .Height(50)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on: ");
                            text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).Bold();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateReceiptHtml(PaymentReceipt receipt)
        {
            var html = GenerateReceiptHtmlContent(receipt);
            return Encoding.UTF8.GetBytes(html);
        }
        
        public string GenerateReceiptHtmlContent(PaymentReceipt receipt)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Payment Receipt - {receipt.ReceiptNumber}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .receipt {{
            max-width: 800px;
            margin: 0 auto;
            background: white;
            padding: 40px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .header h1 {{
            color: #007bff;
            margin: 0;
            font-size: 32px;
        }}
        .header p {{
            color: #666;
            margin: 5px 0;
        }}
        .receipt-number {{
            text-align: right;
            font-size: 18px;
            font-weight: bold;
            color: #007bff;
            margin-bottom: 20px;
        }}
        .section {{
            margin-bottom: 30px;
        }}
        .section-title {{
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 15px;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 5px;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #f0f0f0;
        }}
        .detail-label {{
            font-weight: 600;
            color: #555;
        }}
        .detail-value {{
            color: #333;
        }}
        .payment-amount {{
            background: #e7f3ff;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
            text-align: center;
        }}
        .payment-amount .label {{
            font-size: 14px;
            color: #666;
            margin-bottom: 5px;
        }}
        .payment-amount .amount {{
            font-size: 36px;
            font-weight: bold;
            color: #007bff;
        }}
        .summary-table {{
            width: 100%;
            margin-top: 20px;
            border-collapse: collapse;
        }}
        .summary-table td {{
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
        }}
        .summary-table .label {{
            font-weight: 600;
            color: #555;
            width: 60%;
        }}
        .summary-table .value {{
            text-align: right;
            color: #333;
            font-weight: 600;
        }}
        .summary-table .total {{
            background: #f8f9fa;
            font-size: 18px;
            color: #007bff;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 2px solid #f0f0f0;
            text-align: center;
            color: #666;
            font-size: 14px;
        }}
        .thank-you {{
            background: #d4edda;
            color: #155724;
            padding: 15px;
            border-radius: 5px;
            text-align: center;
            margin-top: 30px;
            font-weight: 600;
        }}
        .stamp {{
            text-align: right;
            margin-top: 40px;
            font-style: italic;
            color: #999;
        }}
    </style>
</head>
<body>
    <div class='receipt'>
        <!-- Header -->
        <div class='header'>
            <h1>🏋️ GYM MANAGEMENT SYSTEM</h1>
            <p>Your Fitness, Our Priority</p>
            <p>Email: info@gym.com | Phone: +91-1234567890</p>
        </div>
        
        <!-- Receipt Number -->
        <div class='receipt-number'>
            Receipt No: {receipt.ReceiptNumber}
        </div>
        
        <!-- Payment Amount Highlight -->
        <div class='payment-amount'>
            <div class='label'>Amount Paid</div>
            <div class='amount'>₹{receipt.AmountPaid:N2}</div>
        </div>
        
        <!-- Member Details -->
        <div class='section'>
            <div class='section-title'>Member Details</div>
            <div class='detail-row'>
                <span class='detail-label'>Member Name:</span>
                <span class='detail-value'>{receipt.MemberName}</span>
            </div>
            <div class='detail-row'>
                <span class='detail-label'>Email:</span>
                <span class='detail-value'>{receipt.MemberEmail}</span>
            </div>
            <div class='detail-row'>
                <span class='detail-label'>Phone:</span>
                <span class='detail-value'>{receipt.MemberPhone}</span>
            </div>
            <div class='detail-row'>
                <span class='detail-label'>Membership Plan:</span>
                <span class='detail-value'>{receipt.PlanName}</span>
            </div>
        </div>
        
        <!-- Payment Details -->
        <div class='section'>
            <div class='section-title'>Payment Details</div>
            <div class='detail-row'>
                <span class='detail-label'>Payment Date:</span>
                <span class='detail-value'>{receipt.PaymentDate:dd MMM yyyy, hh:mm tt}</span>
            </div>
            <div class='detail-row'>
                <span class='detail-label'>Payment Method:</span>
                <span class='detail-value'>{receipt.PaymentMethod ?? "Cash"}</span>
            </div>
            {(string.IsNullOrEmpty(receipt.TransactionId) ? "" : $@"
            <div class='detail-row'>
                <span class='detail-label'>Transaction ID:</span>
                <span class='detail-value'>{receipt.TransactionId}</span>
            </div>")}
            <div class='detail-row'>
                <span class='detail-label'>Received By:</span>
                <span class='detail-value'>{receipt.ReceivedBy}</span>
            </div>
        </div>
        
        <!-- Payment Summary -->
        <div class='section'>
            <div class='section-title'>Payment Summary</div>
            <table class='summary-table'>
                <tr>
                    <td class='label'>Total Membership Amount:</td>
                    <td class='value'>₹{receipt.TotalAmount:N2}</td>
                </tr>
                <tr>
                    <td class='label'>Previously Paid:</td>
                    <td class='value'>₹{receipt.PreviousPaid:N2}</td>
                </tr>
                <tr>
                    <td class='label'>Current Payment:</td>
                    <td class='value' style='color: #28a745;'>₹{receipt.AmountPaid:N2}</td>
                </tr>
                <tr class='total'>
                    <td class='label'>Total Paid:</td>
                    <td class='value'>₹{(receipt.PreviousPaid + receipt.AmountPaid):N2}</td>
                </tr>
                <tr>
                    <td class='label'>Remaining Balance:</td>
                    <td class='value' style='color: {(receipt.RemainingAmount > 0 ? "#dc3545" : "#28a745")};'>₹{receipt.RemainingAmount:N2}</td>
                </tr>
            </table>
        </div>
        
        {(string.IsNullOrEmpty(receipt.Notes) ? "" : $@"
        <!-- Notes -->
        <div class='section'>
            <div class='section-title'>Notes</div>
            <p style='color: #666;'>{receipt.Notes}</p>
        </div>")}
        
        <!-- Thank You -->
        <div class='thank-you'>
            ✅ Thank you for your payment! {(receipt.RemainingAmount <= 0 ? "Your membership is fully paid." : "")}
        </div>
        
        <!-- Stamp -->
        <div class='stamp'>
            This is a computer-generated receipt and does not require a signature.
        </div>
        
        <!-- Footer -->
        <div class='footer'>
            <p>For any queries, please contact us at info@gym.com</p>
            <p>Generated on: {DateTime.UtcNow:dd MMM yyyy, hh:mm tt} UTC</p>
        </div>
    </div>
</body>
</html>";
            
            return html;
        }
    }
}

