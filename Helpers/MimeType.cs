using System;
using System.Collections.Generic;
using ttu_mms_relay.Structures;

namespace ttu_mms_relay.Helpers
{
  public static class MimeType
  {
    private static readonly Dictionary<string, string> m_ValidMimeTypes = new Dictionary<string, string>(){
      {"image/jpeg", ".jpg"},
      {"image/png", ".png"}
    };

    public static string TypeToExtension(string mimeType)
    {
      if (m_ValidMimeTypes.ContainsKey(mimeType.ToLower())) return m_ValidMimeTypes[mimeType];

      throw new NotSupportedException("Mime type not supported: " + mimeType);
    }

    public static bool IsSupported(string mimeType)
    {
      return m_ValidMimeTypes.ContainsKey(mimeType.ToLower());
    }

    public static bool IsSupported(MmsSet message)
    {
      return IsSupported(message.MimeType);
    }
  }
}
