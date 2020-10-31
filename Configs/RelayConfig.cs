using System.Collections.Generic;

namespace ttu_mms_relay.Configs
{
  public class RelayConfig
  {
    /// <summary>
    /// The identifier of the section in appsettings associated with this object
    /// </summary>
    public static readonly string Identifier = "RelayConfig";
    public AccessControlConfig AccessControl { get; set; }
    public DropboxConfig Dropbox { get; set; }
    public TwilioConfig Twilio { get; set; }
  }

  public class AccessControlConfig
  {
    /// <summary>
    /// A list of phone numbers which are 
    /// </summary>
    /// <value></value>
    public List<string> Trusted { get; set; }
    public List<string> Blocked { get; set; }
  }

  public class DropboxConfig
  {
    /// <summary>
    /// The application's generated access token for the target account
    /// </summary>
    /// <value></value>
    public string AccessToken { get; set; }
    /// <summary>
    /// The path to the folder where submitted media files from trusted senders should be uploaded
    /// </summary>
    /// <example>/Events/2020/ThisEvent/Live/
    /// <value></value>
    public string LiveFolder { get; set; }
    /// <summary>
    /// The path to the folder where submitted media files from untrusted senders should be uploaded
    /// </summary>
    /// <example>/Events/2020/ThisEvent/Needs Review/
    /// <value></value>
    public string ReviewFolder { get; set; }
  }

  public class TwilioConfig
  {
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
  }
}
