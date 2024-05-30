using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using publishing.Areas.Identity.Data;
using publishing.Models;
using Quartz;

namespace publishing.Infrastructure
{
    public class GetStartBookings: IJob
    {

        private readonly PublishingDBContext _context;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;

        public GetStartBookings(PublishingDBContext context, UserManager<publishingUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var managers = _userManager.GetUsersInRoleAsync("manager").Result;
            DateTime yesterday = DateTime.Today.AddDays(-1);
            var bookings = _context.Bookings.Include(b=>b.BookingProducts).ThenInclude(bp=>bp.Product).ThenInclude(p=>p.Customer).Where(b => b.Start == yesterday).ToList();

            if (bookings.Count > 0) {
                EmailSender emailSender = new EmailSender();

                foreach (var manager in managers)
                {
                    string subject = "Отчёт о новых заказах";
                    string message = $"Уважаемый, менеджер! Отчёт о новых заказах,составленных {yesterday.ToString("d")}, готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(GetStartBookingsReport(bookings,yesterday.ToString("d")), manager.Email, subject, message);
                }
            }


        }

        private string GetStartBookingsReport(List<Booking> bookings,string date) 
        {
            string pathToWorkPiece = "/Reports/start_bookings_draft.xlsx";
            string result = $"/Reports/start_bookings_result.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(workPiece))
            {
                //устанавливаем поля документа
                excelPackage.Workbook.Properties.Author = "Издательство";
                excelPackage.Workbook.Properties.Title = "Отчёт о новых заказах";
                excelPackage.Workbook.Properties.Subject = "Новые заказы";
                excelPackage.Workbook.Properties.Created = DateTime.Now;
                //плучаем лист по имени.
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["bookings"];
                //получаем списко пользователей и в цикле заполняем лист данными
                worksheet.Cells[2, 3].Value = date;
                int startLine = 4;

                foreach (Booking booking in bookings)
                {
                    worksheet.Cells[startLine, 1].Value = startLine - 3;
                    worksheet.Cells[startLine, 2].Value = booking.Id;
                    worksheet.Cells[startLine, 3].Value = booking.Start.ToString("d");
                    worksheet.Cells[startLine, 4].Value = booking.End == null ? "Отсутствует" : booking.End.Value.ToString("d");
                    worksheet.Cells[startLine, 5].Value = $"{booking.Cost} ₽";
                    worksheet.Cells[startLine, 6].Value = booking.BookingProducts.First().Product.Customer.Name;
                    startLine++;
                }

                excelPackage.SaveAs(fileInfoResult);
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = $"start_bookings_result.xlsx";

            return _appEnvironment.WebRootPath + result;
        }
    }
}
