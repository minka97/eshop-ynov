using Email.API.Features.SendEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

/// <summary>
/// Controller pour gérer l'envoi d'emails via API REST
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<EmailController> _logger;

    public EmailController(ISender sender, ILogger<EmailController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Envoie un email manuellement
    /// </summary>
    /// <param name="command">Commande d'envoi d'email</param>
    /// <returns>Résultat de l'envoi</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendEmailResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendEmailResult>> SendEmail([FromBody] SendEmailCommand command)
    {
        _logger.LogInformation("📬 Réception d'une requête d'envoi d'email");

        var result = await _sender.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
