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
                <div style='font-family: system-ui, -apple-system, sans-serif; max-width: 600px; margin: 0 auto; color: #0f172a;'>
                    <h2 style='color: #ff6600;'>Welcome to Z-A-A-T-T Marketplace</h2>
                    <p>Please confirm your email address to unlock full trading features by clicking the secure link below:</p>
                    <p style='margin: 24px 0;'>
                        <a href='{confirmationLink}' style='background: #ff6600; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Confirm My Email
                        </a>
                    </p>
                    <p style='font-size: 0.85em; color: #64748b;'>If you did not create an account on ZaatMarket, please ignore this email.</p>
                </div>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Reset your password - Z-A-A-T-T Marketplace";
            var htmlMessage = $@"
                <div style='font-family: system-ui, -apple-system, sans-serif; max-width: 600px; margin: 0 auto; color: #0f172a;'>
                    <h2 style='color: #0f172a;'>Password Reset Request</h2>
                    <p>We received a request to reset your ZaatMarket password. Click below to choose a new password:</p>
                    <p style='margin: 24px 0;'>
                        <a href='{resetLink}' style='background: #0f172a; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Reset Password
                        </a>
                    </p>
                </div>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Password Reset Code - Z-A-A-T-T Marketplace";
            var htmlMessage = $@"
                <div style='font-family: system-ui, -apple-system, sans-serif; max-width: 600px; margin: 0 auto; color: #0f172a;'>
                    <p>Your secure password reset code is:</p>
                    <h2 style='background: #f1f5f9; padding: 16px; border-radius: 8px; display: inline-block; letter-spacing: 4px; color: #ff6600;'>{resetCode}</h2>
                </div>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendOfflineChatNotificationAsync(string toEmail, string senderUsername, string messagePreview)
        {
            var subject = $"New message from {senderUsername} on Z-A-A-T-T";
            var htmlMessage = $@"
                <div style='font-family: system-ui, -apple-system, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #111827;'>You have a new message!</h2>
                    <p><b>{senderUsername}</b> sent you a message regarding a listing:</p>
                    
                    <blockquote style='border-left: 4px solid #ff6600; padding-left: 15px; margin-left: 0; color: #475569; font-size: 1.05em; font-style: italic; background: #f8fafc; padding: 16px; border-radius: 0 8px 8px 0;'>
                        ""{messagePreview}""
                    </blockquote>
                    
                    <p style='margin-top: 24px;'>
                        <a href='https://zaattmarket.co.zw/messages' style='background: #ff6600; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            Reply on ZaatMarket
                        </a>
                    </p>
                    <p style='margin-top: 30px; font-size: 0.8em; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 15px;'>
                        You are receiving this because you have unread messages in your marketplace inbox.
                    </p>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }

        public async Task SendOrderReceiptAsync(string toEmail, string customerName, string orderReference, decimal totalAmount, string deliveryFrom, string deliveryTo)
        {
            var subject = $"Order Confirmed - {orderReference}";
            var htmlMessage = $@"
                <div style='font-family: system-ui, -apple-system, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #ff6600;'>Order Confirmed!</h2>
                    <p>Hi {customerName},</p>
                    <p>Thank you for shopping on Z-A-A-T-T Marketplace. Your order has been securely logged.</p>
                    
                    <div style='background: #f8fafc; padding: 20px; border-radius: 12px; margin: 20px 0; border: 1px solid #e2e8f0;'>
                        <h3 style='margin-top: 0; color: #0f172a;'>Order Details</h3>
                        <p><strong>Reference:</strong> {orderReference}</p>
                        <p><strong>Total Amount:</strong> ${totalAmount:N2}</p>
                        
                        <hr style='border: none; border-top: 1px solid #cbd5e1; margin: 16px 0;' />
                        
                        <h3 style='margin-top: 0; color: #0f172a;'>Logistics</h3>
                        <p><strong>From:</strong> {deliveryFrom}</p>
                        <p><strong>Destination:</strong> {deliveryTo}</p>
                    </div>
                    
                    <p style='color: #64748b; font-size: 0.9em;'>The seller has been notified and will prepare your items for delivery immediately.</p>
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

            // CRITICAL RESILIENCE: 10-second timeout stops cloud network lags from freezing UI threads!
            client.Timeout = 10000;

            // CRITICAL CPANEL ADAPTATION: Auto cleanly handles Port 465 (Direct SSL) or Port 587 (STARTTLS)
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}