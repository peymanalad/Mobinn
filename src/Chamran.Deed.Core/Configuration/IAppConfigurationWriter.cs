namespace Chamran.Deed.Configuration
{
    public interface IAppConfigurationWriter
    {
        void Write(string key, string value);
    }
}
