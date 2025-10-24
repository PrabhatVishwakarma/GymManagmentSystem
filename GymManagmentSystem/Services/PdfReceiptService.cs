using System.Text;
using GymManagmentSystem.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace GymManagmentSystem.Services
{
    public class PdfReceiptService
    {
        private readonly IConverter _converter;

        public PdfReceiptService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] GenerateReceiptPdf(PaymentReceipt receipt)
        {
            var htmlContent = GenerateReceiptHtmlContent(receipt);
            
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };
            
            return _converter.Convert(doc);
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

