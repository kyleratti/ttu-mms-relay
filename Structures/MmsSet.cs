namespace ttu_mms_relay.Structures
{
  public class MmsSet
  {
    /// <summary>
    /// The publicly accessible URL of the media object
    /// </summary>
    /// <value></value>
    public string Url { get; private set; }
    /// <summary>
    /// The mime type of the linked media object
    /// </summary>
    /// <value></value>
    public string MimeType { get; private set; }
    public string PhoneNumber { get; private set; }
    public string SmsSid { get; private set; }

    public MmsSet(string url, string mimeType, string phoneNumber, string smsSid)
    {
      this.Url = url;
      this.MimeType = mimeType;
      this.PhoneNumber = phoneNumber;
      this.SmsSid = smsSid;
    }
  }
}
