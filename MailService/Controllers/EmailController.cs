using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(EmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromQuery] string toEmailAddress, [FromQuery] string subject, [FromQuery] string body)
    {
        if (string.IsNullOrEmpty(toEmailAddress) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
        {
            return BadRequest("ToEmailAddress, subject, and body are required.");
        }

        try
        {
            await _emailService.SendEmailAsync(toEmailAddress, subject, body);
            return Ok("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending the email.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
