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
    private readonly ILogger<AiChatService> _logger;
    private const int HistoryLimit = 20;

    public AiChatService(
        IDbContextFactory<AppDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<AiChatService> logger)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ChatResponseDto> Conversar(ChatRequestDto request, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();

        var config = await _context.AiConfigs.FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == request.Provider && c.IsActive)
            ?? throw new ApplicationException($"Nenhuma configuração ativa de {request.Provider} encontrada.");

        var historico = await _context.AiMessages
            .Where(m => m.UserId == userId && m.SessionId == request.SessionId)
            .OrderByDescending(m => m.CreatedAt).Take(HistoryLimit).OrderBy(m => m.CreatedAt).ToListAsync();

        var contextoFinanceiro = await BuildContextoFinanceiro(_context, userId);
        var systemPrompt = BuildSystemPrompt(contextoFinanceiro);

        string resposta = request.Provider switch
        {
            AiProvider.Claude => await ChamarClaude(config.ApiKey, systemPrompt, historico, request.Message),
            AiProvider.Gemini => await ChamarGemini(config.ApiKey, systemPrompt, historico, request.Message),
            _ => throw new ApplicationException("Provedor de IA não suportado.")
        };

        _context.AiMessages.AddRange(
            new AiMessage { UserId = userId, SessionId = request.SessionId, Provider = request.Provider, Role = "user", Content = request.Message },
            new AiMessage { UserId = userId, SessionId = request.SessionId, Provider = request.Provider, Role = "assistant", Content = resposta }
        );
        await _context.SaveChangesAsync();

        return new ChatResponseDto { SessionId = request.SessionId, Provider = request.Provider, Response = resposta };
    }

    public async Task<List<string>> ListarSessoes(int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.AiMessages.Where(m => m.UserId == userId).Select(m => m.SessionId).Distinct().ToListAsync();
    }

    public async Task<List<object>> BuscarHistorico(string sessionId, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        var mensagens = await _context.AiMessages.Where(m => m.UserId == userId && m.SessionId == sessionId).OrderBy(m => m.CreatedAt).ToListAsync();
        return mensagens.Select(m => (object)new { m.Role, m.Content, m.Provider, m.CreatedAt }).ToList();
    }

    public async Task LimparHistorico(string sessionId, int userId)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync();
        var mensagens = await _context.AiMessages.Where(m => m.UserId == userId && m.SessionId == sessionId).ToListAsync();
        _context.AiMessages.RemoveRange(mensagens);
        await _context.SaveChangesAsync();
    }

    // #8: Aggregations pushed to SQL — no longer loads all 12 months of transactions into memory
    private static async Task<string> BuildContextoFinanceiro(AppDbContext context, int userId)
    {
        var hoje = DateTime.UtcNow;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inicioHistorico = inicioMes.AddMonths(-12);

        var contas    = await context.Accounts.Where(a => a.UserId == userId).ToListAsync();
        var todasMetas = await context.MonthlyBudgets.Where(b => b.UserId == userId).ToListAsync();

        var resumoMes = await context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= inicioMes)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Receitas = g.Sum(t => t.Direction == TransactionDirection.income  ? t.Amount : 0),
                Despesas = g.Sum(t => t.Direction == TransactionDirection.expense ? t.Amount : 0)
            })
            .FirstOrDefaultAsync();

        var receitasMes = resumoMes?.Receitas ?? 0;
        var despesasMes = resumoMes?.Despesas ?? 0;

        var resumo = await context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= inicioHistorico && t.TransactionDate < inicioMes)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Receitas = g.Sum(t => t.Direction == TransactionDirection.income  ? t.Amount : 0),
                Despesas = g.Sum(t => t.Direction == TransactionDirection.expense ? t.Amount : 0)
            })
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
            .ToListAsync();

        var despesasPorCategoria = await context.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= inicioMes && t.Direction == TransactionDirection.expense)
            .Select(t => new { CatName = t.Category != null ? t.Category.Name : "Sem categoria", t.Amount })
            .ToListAsync();

        var porCat = despesasPorCategoria
            .GroupBy(x => x.CatName)
            .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(g => g.Total)
            .ToList();

        var ultimas = await context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(20)
            .Select(t => new
            {
                t.TransactionDate,
                t.Direction,
                t.Amount,
                t.Description,
                CatName = t.Category != null ? t.Category.Name : "Sem categoria"
            })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine($"Data atual: {hoje:dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine("=== CONTAS ===");
        foreach (var conta in contas)
            sb.AppendLine($"- {conta.Name}{(conta.HasCreditCard ? " (com cartão de crédito)" : "")}");

        sb.AppendLine();
        sb.AppendLine($"=== MÊS ATUAL ({hoje:MMMM/yyyy}) ===");
        sb.AppendLine($"Receitas: R$ {receitasMes:N2}");
        sb.AppendLine($"Despesas: R$ {despesasMes:N2}");
        sb.AppendLine($"Saldo do mês: R$ {receitasMes - despesasMes:N2}");

        var metaMes = todasMetas.FirstOrDefault(b => b.Month == hoje.Month && b.Year == hoje.Year);
        if (metaMes != null)
        {
            var pct = metaMes.Amount > 0 ? Math.Round((double)(despesasMes / metaMes.Amount * 100), 1) : 0;
            sb.AppendLine($"Meta de gastos: R$ {metaMes.Amount:N2} (utilizado: {pct}%)");
        }
        else sb.AppendLine("Meta de gastos: não definida para este mês");

        if (porCat.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Despesas por categoria (mês atual):");
            foreach (var cat in porCat) sb.AppendLine($"  - {cat.Categoria}: R$ {cat.Total:N2}");
        }

        sb.AppendLine();
        sb.AppendLine("=== HISTÓRICO MENSAL (últimos 12 meses) ===");
        if (!resumo.Any())
            sb.AppendLine("Sem dados históricos anteriores ao mês atual.");
        else
            foreach (var mes in resumo)
            {
                var nome    = new DateTime(mes.Year, mes.Month, 1).ToString("MMMM/yyyy");
                var saldo   = mes.Receitas - mes.Despesas;
                var metaInfo = todasMetas.FirstOrDefault(b => b.Month == mes.Month && b.Year == mes.Year) is { } m
                    ? $" | Meta: R$ {m.Amount:N2}" : "";
                sb.AppendLine($"  {nome}: Receitas R$ {mes.Receitas:N2} | Despesas R$ {mes.Despesas:N2} | Saldo R$ {saldo:N2}{metaInfo}");
            }

        if (ultimas.Any())
        {
            sb.AppendLine();
            sb.AppendLine("=== ÚLTIMAS 20 TRANSAÇÕES ===");
            foreach (var t in ultimas)
            {
                var tipo = t.Direction == TransactionDirection.income ? "+" : "-";
                sb.AppendLine($"  {t.TransactionDate:dd/MM/yyyy} {tipo}R$ {t.Amount:N2} — {t.CatName}{(string.IsNullOrEmpty(t.Description) ? "" : $" ({t.Description})")}");
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

    private async Task<string> ChamarClaude(string apiKey, string systemPrompt, List<AiMessage> historico, string mensagem)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        var messages = historico.Select(m => new { role = m.Role, content = m.Content }).ToList<object>();
        messages.Add(new { role = "user", content = mensagem });
        var body = new { model = "claude-haiku-4-5-20251001", max_tokens = 1024, system = systemPrompt, messages };
        var response = await client.PostAsync("https://api.anthropic.com/v1/messages", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            // #5: Log provider details server-side; never expose raw API response to the client
            _logger.LogError("Erro do provedor Claude: status={Status} body={Body}", response.StatusCode, content);
            throw new ApplicationException("Serviço de IA temporariamente indisponível. Tente novamente.");
        }
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;
    }

    private async Task<string> ChamarGemini(string apiKey, string systemPrompt, List<AiMessage> historico, string mensagem)
    {
        var client = _httpClientFactory.CreateClient();
        // #2: Key moved from URL query param (logged by every proxy/server) to request header
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
        var contents = historico.Select(m => new { role = m.Role == "assistant" ? "model" : "user", parts = new[] { new { text = m.Content } } }).ToList<object>();
        contents.Add(new { role = "user", parts = new[] { new { text = mensagem } } });
        var body = new { systemInstruction = new { parts = new[] { new { text = systemPrompt } } }, contents };
        var response = await client.PostAsync("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            // #5: Log provider details server-side; never expose raw API response to the client
            _logger.LogError("Erro do provedor Gemini: status={Status} body={Body}", response.StatusCode, content);
            throw new ApplicationException("Serviço de IA temporariamente indisponível. Tente novamente.");
        }
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
    }
}
