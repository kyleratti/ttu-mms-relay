using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValidateRequest.Filters;
using ttu_mms_relay.Helpers;
using ttu_mms_relay.Configs;
using Microsoft.Extensions.Configuration;

namespace ttu_mms_relay.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class MmsController : ControllerBase
  {
    private readonly ILogger<MmsController> m_Logger;

    public MmsController(ILogger<MmsController> logger)
    {
      this.m_Logger = logger;
    }

    [HttpPost("receive")]
    [ValidateTwilioRequest]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ReceiveAsync([FromForm] IFormCollection data, [FromServices] IConfiguration config)
    {
      var relayConfig = Configurator.GetSection<RelayConfig>(config, RelayConfig.Identifier);

      // 202 Accepted
      // Webhook was called but not for MMS; technically not an error,
      // but also not something we're going to be processing.
      // This application is strictly for incoming MMS.
      if (!Message.IsMmsRequest(data))
      {
        this.m_Logger.LogInformation("Discarding incoming webhook (not MMS)");
        return Accepted();
      }

      var attachments = Message.GetMmsAttachments(data);

      this.m_Logger.LogDebug(String.Format("Found {0} attachment{1}", attachments.Count, attachments.Count != 1 ? "s" : ""));

      try
      {
        foreach (var attachment in attachments)
        {
          if (!MimeType.IsSupported(attachment))
          {
            this.m_Logger.LogInformation(String.Format("Skipping attachment {0}: MimeType ({1}) not supported", attachment.SmsSid, attachment.MimeType));
            continue;
          }

          var processor = new MediaProcessor(attachment, relayConfig);

          this.m_Logger.LogDebug(attachment.Url + ": Download starting");
          await processor.Download();
          this.m_Logger.LogDebug(attachment.Url + ": Download finished");

          this.m_Logger.LogDebug(attachment.Url + ": Dropbox upload starting");
          await processor.UploadToDropbox();
          this.m_Logger.LogDebug(attachment.Url + ": Dropbox upload finished");

          this.m_Logger.LogDebug(attachment.Url + ": Twilio purge starting");
          processor.RemoveFromTwilio();
          this.m_Logger.LogDebug(attachment.Url + ": Twilio purge finished");

          this.m_Logger.LogDebug(attachment.Url + ": CleanUp starting");
          processor.CleanUp();
          this.m_Logger.LogDebug(attachment.Url + ": CleanUp finished");
        }
      }
      catch (Exception ex)
      {
        this.m_Logger.LogError(ex, "Error processing media");

        return StatusCode(StatusCodes.Status502BadGateway, Problem("Error processing media", null, StatusCodes.Status502BadGateway));
      }

      this.m_Logger.LogDebug(String.Format("Successfully processed {0} media object{1}", attachments.Count, attachments.Count != 1 ? "s" : ""));

      return StatusCode(StatusCodes.Status202Accepted);
    }
  }
}
