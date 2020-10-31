using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using ttu_mms_relay.Configs;
using ttu_mms_relay.Structures;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ttu_mms_relay.Helpers
{
  public class MediaProcessor
  {
    private MmsSet Media { get; set; }
    private RelayConfig RelayConfig { get; set; }
    private string FilePath { get; set; }


    public MediaProcessor(MmsSet media, RelayConfig relayConfig)
    {
      this.Media = media;
      this.RelayConfig = relayConfig;
    }

    /// <summary>
    /// Downloads the associated media file from Twilio and saves it to a random path on disk
    /// </summary>
    /// <returns>A Task of the download</returns>
    public Task Download()
    {
      this.FilePath = Path.GetTempFileName();
      var net = new WebClient();
      return net.DownloadFileTaskAsync(new Uri(this.Media.Url), this.FilePath);
    }

    /// <summary>
    /// Uploads the downloaded file to Dropbox
    /// </summary>
    /// <returns></returns>
    public Task<FileMetadata> UploadToDropbox()
    {

      var client = new DropboxClient(this.RelayConfig.Dropbox.AccessToken);
      // ONLY need an access key here

      var fileExt = Path.GetExtension(this.FilePath);
      var fileName = Path.GetFileNameWithoutExtension(this.FilePath);
      var newFileName = String.Format("{0}_{1}{2}{3}", this.Media.PhoneNumber.Replace("+", ""), fileName, fileExt, MimeType.TypeToExtension(this.Media.MimeType));

      // This is a dirty workaround. We want to be able to await our upload
      // in the controller before continuing, however if we simply return
      // the task from UploadAsync, the stream will be discarded before the
      // upload can finish, resulting in an ApiException.
      return Task.Run(async () =>
      {
        using var stream = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read);
        return await client.Files.UploadAsync("/__testing/needs review/" + newFileName, WriteMode.Add.Instance, body: stream);
      });
    }

    public void RemoveFromTwilio()
    {
      // Do not delete if we are in the dev environment
      // This saves us from spending a penny every time you need to run a test
      if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower() != "development")
      {
        TwilioClient.Init(this.RelayConfig.Twilio.AccountSid, this.RelayConfig.Twilio.AuthToken);
        MessageResource.Delete(pathSid: this.Media.SmsSid);
      }
    }

    public void CleanUp()
    {
      File.Delete(this.FilePath);
    }
  }
}
