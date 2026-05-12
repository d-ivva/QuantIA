using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuantIA.Data;
using QuantIA.DTOs;
using QuantIA.Interface;
using QuantIA.Models;

namespace QuantIA.Services;

public class AiChatService : IAiChatService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    private const int HistoryLimit = 20;

    public AiChatService(IDbContextFactory<AppDbContext> contextFactory, IHttpClientFactory httpClientFactory)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ChatResponseDto> Conversar(ChatRequestDto request)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var config = await _context.AiConfigs
            .FirstOrDefaultAsync(c => c.Provider == request.Provider && c.IsActive)
            ?? throw new ApplicationException($"Nenhuma configuração ativa de {request.Provider} encontrada.");

        var historico = await _context.AiMessages
            .Where(m => m.SessionId == request.SessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(HistoryLimit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var contextoFinanceiro = await BuildContextoFinanceiro(_context);
        var systemPrompt = BuildSystemPrompt(contextoFinanceiro);

        string resposta = request.Provider switch
        {
            AiProvider.Claude => await ChamarClaude(config.ApiKey, systemPrompt, historico, request.Message),
            AiProvider.Gemini => await ChamarGemini(config.ApiKey, systemPrompt, historico, request.Message),
            _ => throw new ApplicationException("Provedor de IA não suportado.")
        };

        _context.AiMessages.AddRange(
            new AiMessage
            {
                SessionId = request.SessionId,
                Provider = request.Provider,
                Role = "user",
                Content = request.Message
            },
            new AiMessage
            {
                SessionId = request.SessionId,
                Provider = request.Provider,
                Role = "assistant",
                Content = resposta
            }
        );

        await _context.SaveChangesAsync();

        return new ChatResponseDto
        {
            SessionId = request.SessionId,
            Provider = request.Provider,
            Response = resposta
        };
    }

    public async Task<List<string>> ListarSessoes()
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        return await _context.AiMessages
            .Select(m => m.SessionId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<object>> BuscarHistorico(string sessionId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var mensagens = await _context.AiMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return mensagens.Select(m => (object)new
        {
            m.Role,
            m.Content,
            m.Provider,
            m.CreatedAt
        }).ToList();
    }

    public async Task LimparHistorico(string sessionId)
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var mensagens = await _context.AiMessages
            .Where(m => m.SessionId == sessionId)
            .ToListAsync();

        _context.AiMessages.RemoveRange(mensagens);
        await _context.SaveChangesAsync();
    }

    // ─── Contexto financeiro ───────────────────────────────────────────

    private static async Task<string> BuildContextoFinanceiro(AppDbContext context)
    {
        var hoje = DateTime.UtcNow;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inicioHistorico = inicioMes.AddMonths(-12);

        var todasTransacoes = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= inicioHistorico)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        var todasMetas = await context.MonthlyBudgets.ToListAsync();

        var contas = await context.Accounts.ToListAsync();

        var sb = new StringBuilder();

        sb.AppendLine($"Data atual: {hoje:dd/MM/yyyy}");
        sb.AppendLine();

        // Contas
        sb.AppendLine("=== CONTAS ===");
        foreach (var conta in contas)
            sb.AppendLine($"- {conta.Name}{(conta.HasCreditCard ? " (com cartão de crédito)" : "")}");

        // Mês atual
        sb.AppendLine();
        sb.AppendLine($"=== MÊS ATUAL ({hoje:MMMM/yyyy}) ===");

        var transacoesMes = todasTransacoes.Where(t => t.TransactionDate >= inicioMes).ToList();
        var receitasMes = transacoesMes.Where(t => t.Direction == TransactionDirection.income).Sum(t => t.Amount);
        var despesasMes = transacoesMes.Where(t => t.Direction == TransactionDirection.expense).Sum(t => t.Amount);

        sb.AppendLine($"Receitas: R$ {receitasMes:N2}");
        sb.AppendLine($"Despesas: R$ {despesasMes:N2}");
        sb.AppendLine($"Saldo do mês: R$ {receitasMes - despesasMes:N2}");

        var metaMes = todasMetas.FirstOrDefault(b => b.Month == hoje.Month && b.Year == hoje.Year);
        if (metaMes != null)
        {
            var percentual = metaMes.Amount > 0 ? Math.Round((double)(despesasMes / metaMes.Amount * 100), 1) : 0;
            sb.AppendLine($"Meta de gastos: R$ {metaMes.Amount:N2} (utilizado: {percentual}%)");
        }
        else
        {
            sb.AppendLine("Meta de gastos: não definida para este mês");
        }

        var porCategoriaMes = transacoesMes
            .Where(t => t.Direction == TransactionDirection.expense)
            .GroupBy(t => t.Category?.Name ?? "Sem categoria")
            .Select(g => new { Categoria = g.Key, Total = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Total)
            .ToList();

        if (porCategoriaMes.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Despesas por categoria (mês atual):");
            foreach (var cat in porCategoriaMes)
                sb.AppendLine($"  - {cat.Categoria}: R$ {cat.Total:N2}");
        }

        // Histórico mensal dos últimos 12 meses
        sb.AppendLine();
        sb.AppendLine("=== HISTÓRICO MENSAL (últimos 12 meses) ===");

        var resumoPorMes = todasTransacoes
            .Where(t => t.TransactionDate < inicioMes)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Receitas = g.Where(t => t.Direction == TransactionDirection.income).Sum(t => t.Amount),
                Despesas = g.Where(t => t.Direction == TransactionDirection.expense).Sum(t => t.Amount),
            })
            .ToList();

        if (!resumoPorMes.Any())
        {
            sb.AppendLine("Sem dados históricos anteriores ao mês atual.");
        }
        else
        {
            foreach (var mes in resumoPorMes)
            {
                var nomeMes = new DateTime(mes.Year, mes.Month, 1).ToString("MMMM/yyyy");
                var saldo = mes.Receitas - mes.Despesas;
                var metaHist = todasMetas.FirstOrDefault(b => b.Month == mes.Month && b.Year == mes.Year);
                var metaInfo = metaHist != null ? $" | Meta: R$ {metaHist.Amount:N2}" : "";
                sb.AppendLine($"  {nomeMes}: Receitas R$ {mes.Receitas:N2} | Despesas R$ {mes.Despesas:N2} | Saldo R$ {saldo:N2}{metaInfo}");
            }
        }

        // Últimas 20 transações (todos os meses)
        var ultimasTransacoes = todasTransacoes.Take(20).ToList();
        if (ultimasTransacoes.Any())
        {
            sb.AppendLine();
            sb.AppendLine("=== ÚLTIMAS 20 TRANSAÇÕES ===");
            foreach (var t in ultimasTransacoes)
            {
                var tipo = t.Direction == TransactionDirection.income ? "+" : "-";
                sb.AppendLine($"  {t.TransactionDate:dd/MM/yyyy} {tipo}R$ {t.Amount:N2} — {t.Category?.Name ?? "Sem categoria"}{(string.IsNullOrEmpty(t.Description) ? "" : $" ({t.Description})")}");
            }
        }

        return sb.ToString();
    }

    private static string BuildSystemPrompt(string contextoFinanceiro) => $"""
        Você é QuantIA, um assistente de educação financeira integrado ao aplicativo de finanças pessoais do usuário.

        REGRAS ABSOLUTAS:
        1. Responda APENAS perguntas relacionadas a educação financeira, finanças pessoais, orçamento, economia, investimentos e análise dos dados financeiros do usuário.
        2. Se o usuário perguntar sobre qualquer outro assunto (tecnologia, receitas, saúde, entretenimento, etc.), recuse educadamente e redirecione para temas financeiros.
        3. Use os dados financeiros abaixo para personalizar e enriquecer suas respostas quando relevante.
        4. Responda sempre em português do Brasil.
        5. Seja objetivo, prático e didático.

        DADOS FINANCEIROS DO USUÁRIO:
        {contextoFinanceiro}
        """;

    // ─── Claude ───────────────────────────────────────────────────────

    private async Task<string> ChamarClaude(string apiKey, string systemPrompt, List<AiMessage> historico, string mensagem)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var messages = historico
            .Select(m => new { role = m.Role, content = m.Content })
            .ToList<object>();
        messages.Add(new { role = "user", content = mensagem });

        var body = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 1024,
            system = systemPrompt,
            messages
        };

        var json = JsonSerializer.Serialize(body);
        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Erro ao chamar Claude: {content}");

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    // ─── Gemini ───────────────────────────────────────────────────────

    private async Task<string> ChamarGemini(string apiKey, string systemPrompt, List<AiMessage> historico, string mensagem)
    {
        var client = _httpClientFactory.CreateClient();

        var contents = historico.Select(m => new
        {
            role = m.Role == "assistant" ? "model" : "user",
            parts = new[] { new { text = m.Content } }
        }).ToList<object>();

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = mensagem } }
        });

        var body = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents
        };

        var json = JsonSerializer.Serialize(body);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var response = await client.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Erro ao chamar Gemini: {content}");

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}
