using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ttu_mms_relay.Structures;

namespace ttu_mms_relay.Helpers
{
  public static class Message
  {
    private static readonly string[] m_SmsFields = {
      "ToCountry",
      "MediaContentType0",
      "ToState",
      "SmsMessageSid",
      "NumMedia",
      "ToCity",
      "FromZip",
      "SmsSid",
      "FromState",
      "SmsStatus",
      "FromCity",
      "Body",
      "FromCountry",
      "To",
      "ToZip",
      "NumSegments",
      "MessageSid",
      "AccountSid",
      "From",
      "MediaUrl0",
      "ApiVersion"
    };

    public static bool IsSmsRequest(IFormCollection data)
    {
      foreach (var name in m_SmsFields)
      {
        if (!data.ContainsKey(name)) return false;
      }

      return true;
    }

    public static bool IsMmsRequest(IFormCollection data)
    {
      // If it's not a valid SMS request, it can't be a valid MMS request
      if (!IsSmsRequest(data)) return false;

      // The only way, so far as I can tell, to determine if the form data
      // is from an MMS request is to check if media is attached.
      // Therefore, if _at least_ "MediaUrl0" and "MediaContentType0" exist
      // on the data, it's MMS.
      return data.ContainsKey("MediaUrl0") && data.ContainsKey("MediaContentType0");
    }

    /// <summary>
    /// Returns the total number of MMS attachments on the object
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int CountMmsAttachments(IFormCollection data)
    {
      if (data.ContainsKey("NumMedia"))
      {

        if (Int32.TryParse(data["NumMedia"], out int result)) return result;

        throw new ArithmeticException("Unable to convert NumMedia to number");
      }

      throw new MissingFieldException("Field NumMedia not included (is this an MMS request?)");
    }

    public static IReadOnlyList<MmsSet> GetMmsAttachments(IFormCollection data)
    {
      if (!IsMmsRequest(data)) throw new InvalidOperationException("Object is not an MMS request");

      var numAttachments = CountMmsAttachments(data);
      var attachments = new List<MmsSet>();

      for (int i = 0; i < numAttachments; i++)
      {
        var url = data["MediaUrl" + i];
        var mimeType = data["MediaContentType" + i];
        attachments.Add(new MmsSet(url, mimeType, data["From"], data["SmsSid"]));
      }

      return attachments;
    }
  }
}
