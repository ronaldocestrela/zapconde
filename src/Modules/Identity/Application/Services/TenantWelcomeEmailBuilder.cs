using BuildingBlocks.Shared.Email;

namespace Modules.Identity.Application.Services;

/// <summary>
/// Construtor de mensagens de e-mail de boas-vindas e credenciais de acesso para novos tenants.
/// </summary>
public static class TenantWelcomeEmailBuilder
{
    /// <summary>
    /// Gera a mensagem de e-mail formatada em HTML e texto puro para o síndico / admin master do novo tenant.
    /// </summary>
    public static EmailMessage BuildWelcomeEmail(
        string recipientEmail,
        string recipientName,
        string condoName,
        int tenantId,
        string temporaryPassword)
    {
        var subject = $"Bem-vindo ao SmartCondo - Credenciais do Condomínio {condoName}";

        var bodyText = $"""
            Olá {recipientName},

            Seja bem-vindo ao SmartCondo! Seu cadastro de condomínio foi realizado com sucesso.

            Detalhes da sua conta:
            - Condomínio: {condoName}
            - Tenant ID: {tenantId}
            - E-mail de acesso: {recipientEmail}
            - Senha temporária: {temporaryPassword}

            Por favor, acesse a plataforma e altere sua senha no primeiro login.

            Atenciosamente,
            Equipe SmartCondo / ZapCond
            """;

        var bodyHtml = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; color: #1f2937; margin: 0; padding: 20px; }
                    .card { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 32px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border: 1px solid #e5e7eb; }
                    .header { text-align: center; border-bottom: 2px solid #0066ff; padding-bottom: 16px; margin-bottom: 24px; }
                    .header h1 { color: #0066ff; margin: 0; font-size: 24px; font-weight: 700; }
                    .content { line-height: 1.6; font-size: 15px; }
                    .credentials-box { background-color: #f8fafc; border-left: 4px solid #0066ff; border-radius: 6px; padding: 16px; margin: 20px 0; }
                    .credentials-item { margin: 8px 0; }
                    .credentials-label { font-weight: 600; color: #4b5563; }
                    .credentials-value { font-family: monospace; font-size: 16px; color: #111827; background: #e2e8f0; padding: 2px 6px; border-radius: 4px; }
                    .footer { text-align: center; margin-top: 32px; font-size: 13px; color: #6b7280; border-top: 1px solid #e5e7eb; padding-top: 16px; }
                </style>
            </head>
            <body>
                <div class="card">
                    <div class="header">
                        <h1>SmartCondo / ZapCond</h1>
                    </div>
                    <div class="content">
                        <p>Olá <strong>{{recipientName}}</strong>,</p>
                        <p>Seja bem-vindo ao <strong>SmartCondo</strong>! O processo de onboarding do seu condomínio foi concluído com sucesso.</p>
                        
                        <div class="credentials-box">
                            <div class="credentials-item"><span class="credentials-label">Condomínio:</span> <strong>{{condoName}}</strong></div>
                            <div class="credentials-item"><span class="credentials-label">Tenant ID:</span> <strong>{{tenantId}}</strong></div>
                            <div class="credentials-item"><span class="credentials-label">E-mail de Acesso:</span> <span class="credentials-value">{{recipientEmail}}</span></div>
                            <div class="credentials-item"><span class="credentials-label">Senha Temporária:</span> <span class="credentials-value">{{temporaryPassword}}</span></div>
                        </div>

                        <p>Recomendamos que você efetue o login no sistema e altere sua senha no primeiro acesso por motivos de segurança.</p>
                    </div>
                    <div class="footer">
                        <p>© {{DateTime.UtcNow.Year}} SmartCondo SaaS. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        return new EmailMessage(recipientEmail, subject, bodyHtml, bodyText);
    }
}
