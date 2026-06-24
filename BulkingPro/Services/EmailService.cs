using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BulkingPro.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string name, string code)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            
            using var client = new SmtpClient(smtpSettings["SmtpHost"])
            {
                Port = int.Parse(smtpSettings["SmtpPort"] ?? "587"),
                Credentials = new NetworkCredential(
                    smtpSettings["SenderEmail"],
                    smtpSettings["SenderPassword"]
                ),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true"),
                Timeout = 30000
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtpSettings["SenderEmail"] ?? "suportebulkingpro@gmail.com", "BulkingPro"),
                Subject = "Recuperação de Senha - BulkingPro",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #0a0a0a; color: #ffffff; border-radius: 12px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <img src='https://bulkingpro.com/img/Logo_sem_fundo.png' alt='BulkingPro' style='height: 60px;' />
                            <h1 style='color: #ee1920; font-size: 24px; margin: 10px 0 0;'>Recuperação de Senha</h1>
                        </div>
                        
                        <p style='font-size: 15px; color: #c4c4c4;'>Olá, <strong>{name}</strong>!</p>
                        
                        <p style='font-size: 14px; color: #a0a0a0; line-height: 1.6;'>
                            Recebemos uma solicitação para redefinir sua senha no <strong style='color: #ee1920;'>BulkingPro</strong>.
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='display: inline-block; background: #1a1a1a; border: 2px solid #ee1920; border-radius: 12px; padding: 16px 32px;'>
                                <span style='font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #ee1920; font-family: monospace;'>
                                    {code}
                                </span>
                            </div>
                            <p style='font-size: 12px; color: #666666; margin-top: 10px;'>
                                Este código expira em <strong style='color: #c4a87a;'>15 minutos</strong>
                            </p>
                        </div>
                        
                        <p style='font-size: 13px; color: #888888; line-height: 1.5;'>
                            Caso você não tenha solicitado esta recuperação, ignore este e-mail.
                            <br />
                            Se você tiver dúvidas, entre em contato com nosso suporte.
                        </p>
                        
                        <hr style='border: 1px solid #1a1a1a; margin: 25px 0;' />
                        
                        <p style='font-size: 12px; color: #555555; text-align: center;'>
                            © 2026 <strong style='color: #ee1920;'>BulkingPro</strong> — Sua academia inteligente
                            <br />
                            <span style='font-size: 11px;'>Este é um e-mail automático, não responda.</span>
                        </p>
                    </div>
                "
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
            _logger.LogInformation($"E-mail de recuperação enviado para {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar e-mail de recuperação para {email}");
            return false;
        }
    }
}