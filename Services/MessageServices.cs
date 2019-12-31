using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula.SimpleMembership
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            throw new Exception("Finish This - SendEmailAsync");
            // Plug in your email service here to send an email.
            //return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            throw new Exception("Finish This - SendSmsAsync");
            // Plug in your SMS service here to send a text message.
            //return Task.FromResult(0);
        }
    }
}
