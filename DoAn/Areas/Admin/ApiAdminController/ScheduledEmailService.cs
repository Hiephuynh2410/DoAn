using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using DoAn.Models;
using MimeKit.Utils;

public class ScheduledEmailService : BackgroundService
{
    //toi lich làm gửi gmail

    private readonly IServiceProvider _serviceProvider;
    DlctContext db = new DlctContext();
    public ScheduledEmailService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime targetDate = DateTime.Today.AddDays(1);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DlctContext>();

                var staffMembersWithUpcomingSchedules = dbContext.Staff
                    .Include(s => s.Scheduledetails)
                    .Where(s => s.Scheduledetails.Any(sd => sd.Date == targetDate))
                    .ToList();
                
                foreach (var staffMember in staffMembersWithUpcomingSchedules)
                {
                    var message = new MimeMessage();
                   
                    message.From.Add(new MailboxAddress("Admin", "huynhhiepvan1998@gmail.com"));
                    message.Subject = "Upcoming Work Schedule Notification";
                   
                    message.Body = new TextPart("plain")
                    {
                        Text = "Đi làm đi bạn eiii bớt lười!!!!."
                    };

                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, false);
                        client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                        message.To.Add(new MailboxAddress(staffMember.Name, staffMember.Email));

                        client.Send(message);

                        client.Disconnect(true);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}