using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public TutorController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] TutorRequest request)
        {
            if (request.History?.Count > 20)
                return BadRequest(new { error = "Conversation history is too long." });

            // Load lesson context
            string lessonContext = "";
            if (request.LessonId.HasValue)
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Category)
                    .FirstOrDefaultAsync(l => l.Id == request.LessonId.Value && l.IsPublished);

                if (lesson != null)
                {
                    // Strip HTML tags for context (simple approach)
                    var plainContent = System.Text.RegularExpressions.Regex.Replace(
                        lesson.Content ?? "", "<.*?>", " ");
                    plainContent = System.Text.RegularExpressions.Regex.Replace(
                        plainContent, @"\s+", " ").Trim();

                    // Limit context size to ~3000 chars to fit in prompt
                    if (plainContent.Length > 3000)
                        plainContent = plainContent.Substring(0, 3000) + "...";

                    lessonContext = $"Current lesson: {lesson.Title}" +
                        (string.IsNullOrEmpty(lesson.JapaneseTitle) ? "" : $" ({lesson.JapaneseTitle})") +
                        $"\nCategory: {lesson.Category?.Name}" +
                        $"\nLevel: {(lesson.DifficultyLevel <= 1 ? "Beginner" : lesson.DifficultyLevel == 2 ? "Elementary" : lesson.DifficultyLevel == 3 ? "Intermediate" : "Advanced")}" +
                        $"\n\nLesson content summary:\n{plainContent}";
                }
            }

            // Build messages for the LLM
            var systemPrompt = @"You are ""Sensei"", a friendly and patient Japanese language tutor AI for NihongoAcademy. " +
                "Your role is to help learners understand Japanese lessons, provide examples, quiz them, and help with pronunciation. " +
                "Keep answers concise (2-4 sentences unless explaining something complex). " +
                "Use romaji alongside Japanese text for beginners. " +
                "Be encouraging and warm. If asked about non-Japanese topics, gently redirect. " +
                "If you're unsure about something, say so honestly rather than guessing.\n\n" +
                (string.IsNullOrEmpty(lessonContext) ? "No specific lesson context available." : lessonContext);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add conversation history
            if (request.History != null)
            {
                foreach (var msg in request.History
                    .Where(m => m.Role is "user" or "assistant")
                    .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                    .TakeLast(10))
                {
                    messages.Add(new { role = msg.Role, content = msg.Content[..Math.Min(msg.Content.Length, 1000)] });
                }
            }

            // Add current message
            messages.Add(new { role = "user", content = request.Message });

            // Call LLM API
            var apiKey = _config["AI:TutorApiKey"] ?? "";
            var apiEndpoint = _config["AI:TutorApiEndpoint"] ?? "";
            var model = _config["AI:TutorModel"] ?? "gpt-4o-mini";

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiEndpoint))
            {
                // Fallback: rule-based responses when no API is configured
                var fallbackReply = GenerateFallbackReply(request.Message, lessonContext);
                return Ok(new { reply = fallbackReply });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var payload = new
                {
                    model = model,
                    messages = messages,
                    max_tokens = 500,
                    temperature = 0.7
                };

                var response = await client.PostAsJsonAsync(apiEndpoint, payload);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new { reply = "Sorry, I'm having trouble connecting right now. Please try again in a moment." });
                }

                // Parse OpenAI-compatible response
                using var doc = JsonDocument.Parse(responseBody);
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "I'm not sure how to answer that.";

                return Ok(new { reply = reply.Trim() });
            }
            catch
            {
                return Ok(new { reply = GenerateFallbackReply(request.Message, lessonContext) });
            }
        }

        private static string GenerateFallbackReply(string message, string lessonContext)
        {
            var lower = message.ToLower();

            if (lower.Contains("explain") || lower.Contains("simpler") || lower.Contains("what is"))
            {
                if (lessonContext.Contains("Hiragana"))
                    return "Hiragana is the basic Japanese syllabary — each character represents a syllable (like あ = 'a', か = 'ka'). Start by learning the five vowel row (あいうえお), then build from there!";
                if (lessonContext.Contains("Katakana"))
                    return "Katakana is used for foreign loanwords, onomatopoeia, and emphasis. It has the same sounds as Hiragana but different shapes — for example, ア (katakana) vs あ (hiragana) both represent 'a'.";
                if (lessonContext.Contains("Grammar"))
                    return "Japanese grammar follows SOV order (Subject-Object-Verb), unlike English SVO. Particles like は (wa) and を (o) mark grammatical roles. Start simple and build up!";
                return "This lesson covers key Japanese concepts. Try reading through it step by step, and let me know which part you'd like me to explain in more detail!";
            }

            if (lower.Contains("example") || lower.Contains("more"))
            {
                return "Here are some examples:\n• こんにちは (konnichiwa) — Hello\n• ありがとう (arigatou) — Thank you\n• すみません (sumimasen) — Excuse me\n\nWould you like examples related to a specific topic in this lesson?";
            }

            if (lower.Contains("quiz") || lower.Contains("test"))
            {
                return "Quiz time! Try this:\n1. What sound does か represent?\n2. How do you read し — 'si' or 'shi'?\n3. What is the Japanese word for 'hello'?\n\nType your answers and I'll check them!";
            }

            if (lower.Contains("pronunciation") || lower.Contains("romanization") || lower.Contains("romaji"))
            {
                return "Japanese pronunciation tips:\n• Each kana = one syllable, one sound\n• は is pronounced 'wa' when used as a particle\n• ふ is more like 'fu' (soft f)\n• ん is the only consonant-only character\n\nWhich character's pronunciation would you like me to explain?";
            }

            return "That's a great question! To give you the best answer, I'd love a bit more context. Are you asking about a specific character, grammar rule, or vocabulary word from this lesson?";
        }
    }

    public class TutorRequest
    {
        [Required, StringLength(1000, MinimumLength = 1)]
        public string Message { get; set; } = "";
        public int? LessonId { get; set; }
        public List<TutorMessage>? History { get; set; }
    }

    public class TutorMessage
    {
        [Required, RegularExpression("user|assistant")]
        public string Role { get; set; } = "";
        [Required, StringLength(1000)]
        public string Content { get; set; } = "";
    }
}
