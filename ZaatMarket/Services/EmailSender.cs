using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using ZaatMarket.Data;
using ZaatMarket.Models;

namespace ZaatMarket.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly SmtpSettings _smtp;

        public EmailSender(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "Confirm your email - Z-A-A-T-T Marketplace";
            var htmlMessage = $@"
                <h2>Welcome to Z-A-A-T-T</h2>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Reset your password";
            var htmlMessage = $@"
                <p>You requested a password reset.</p>
                <p><a href='{resetLink}'>Reset Password</a></p>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Password Reset Code";
            var htmlMessage = $@"
                <p>Your password reset code is:</p>
                <h2>{resetCode}</h2>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendOfflineChatNotificationAsync(string toEmail, string senderUsername, string messagePreview)
        {
            var subject = $"New message from {senderUsername} on Z-A-A-T-T";
            var htmlMessage = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #111827;'>You have a new message!</h2>
                    <p><b>{senderUsername}</b> sent you a message regarding a listing:</p>
                    
                    <blockquote style='border-left: 4px solid #ff6600; padding-left: 15px; margin-left: 0; color: #555; font-size: 1.1em; font-style: italic; background: #f9fafb; padding: 15px; border-radius: 4px;'>
                        ""{messagePreview}""
                    </blockquote>
                    
                    <p style='margin-top: 30px;'>
                        <a href='https://zaattmarket.co.zw/messages' style='background: #ff6600; color: white; padding: 12px 25px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Click here to reply
                        </a>
                    </p>
                    <p style='margin-top: 30px; font-size: 0.8em; color: #888; border-top: 1px solid #eee; padding-top: 15px;'>
                        You are receiving this because you have unread messages on ZaatMarket.
                    </p>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendOrderReceiptAsync(string toEmail, string customerName, string orderReference, decimal totalAmount, string deliveryFrom, string deliveryTo)
        {
            var subject = $"Order Confirmed - {orderReference}";
            var htmlMessage = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #ff6600;'>Order Confirmed!</h2>
                    <p>Hi {customerName},</p>
                    <p>Thank you for your order on Z-A-A-T-T Marketplace. We have successfully received your request.</p>
                    
                    <div style='background: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0; border: 1px solid #e2e8f0;'>
                        <h3 style='margin-top: 0; color: #0f172a;'>Order Details</h3>
                        <p><strong>Order Reference:</strong> {orderReference}</p>
                        <p><strong>Total Amount:</strong> ${totalAmount:N2}</p>
                        
                        <hr style='border: none; border-top: 1px solid #cbd5e1; margin: 15px 0;' />
                        
                        <h3 style='margin-top: 0; color: #0f172a;'>Logistics</h3>
                        <p><strong>From:</strong> {deliveryFrom}</p>
                        <p><strong>Final Destination (TO):</strong> {deliveryTo}</p>
                    </div>
                    
                    <p>The seller has been notified and will prepare your items for delivery.</p>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            // --> CRITICAL RESILIENCE FIX: Prevent server hangs by enforcing a strict 10-second timeout!
            client.Timeout = 10000;

            // Optional: Uncomment only if testing locally and certificate validation fails
            // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // --> CRITICAL CPANEL FIX: Using SecureSocketOptions.Auto automatically adapts to Port 465 (SSL) or Port 587 (TLS)
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}