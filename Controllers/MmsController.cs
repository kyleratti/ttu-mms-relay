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
    private ILogger<MmsController> Logger { get; set; }

    public MmsController(ILogger<MmsController> logger)
    {
      this.Logger = logger;
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
        this.Logger.LogInformation("Discarding incoming webhook (not MMS)");
        return Accepted();
      }

      var attachments = Message.GetMmsAttachments(data);

      this.Logger.LogDebug(String.Format("Found {0} attachment{1}", attachments.Count, attachments.Count != 1 ? "s" : ""));

      try
      {
        foreach (var attachment in attachments)
        {
          if (!MimeType.IsSupported(attachment))
          {
            this.Logger.LogInformation(String.Format("Skipping attachment {0}: MimeType ({1}) not supported", attachment.SmsSid, attachment.MimeType));
            continue;
          }

          var blocked = relayConfig.AccessControl.Blocked.IndexOf(attachment.PhoneNumber) != -1;

          if (blocked)
          {
            this.Logger.LogInformation(String.Format("Skipping attachment {0} from {1} (number is blocked)", attachment.SmsSid, attachment.PhoneNumber));
            continue;
          }

          var processor = new MediaProcessor(attachment, relayConfig);

          this.Logger.LogDebug(attachment.Url + ": Download starting");
          await processor.Download();
          this.Logger.LogDebug(attachment.Url + ": Download finished");

          this.Logger.LogDebug(attachment.Url + ": Dropbox upload starting");
          await processor.UploadToDropbox();
          this.Logger.LogDebug(attachment.Url + ": Dropbox upload finished");

          this.Logger.LogDebug(attachment.Url + ": Twilio purge starting");
          processor.RemoveFromTwilio();
          this.Logger.LogDebug(attachment.Url + ": Twilio purge finished");

          this.Logger.LogDebug(attachment.Url + ": CleanUp starting");
          processor.CleanUp();
          this.Logger.LogDebug(attachment.Url + ": CleanUp finished");
        }
      }
      catch (Exception ex)
      {
        this.Logger.LogError(ex, "Error processing media");

        return StatusCode(StatusCodes.Status502BadGateway, Problem("Error processing media", null, StatusCodes.Status502BadGateway));
      }

      this.Logger.LogDebug(String.Format("Successfully processed {0} media object{1}", attachments.Count, attachments.Count != 1 ? "s" : ""));

      return StatusCode(StatusCodes.Status202Accepted);
    }
  }
}
