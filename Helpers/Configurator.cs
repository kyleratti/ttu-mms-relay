using Microsoft.Extensions.Configuration;

namespace ttu_mms_relay.Helpers
{
  public static class Configurator
  {
    public static T GetSection<T>(IConfiguration config, string key)
    {
      return config.GetSection(key).Get<T>();
    }
  }
}
