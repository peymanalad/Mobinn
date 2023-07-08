namespace Chamran.Deed.Web.Helpers.StimulsoftHelpers;

public class CommandJson
{
    public string Command { get; set; }

    public string ConnectionString { get; set; }

    public string Database { get; set; }

    public string QueryString { get; set; }

    public int Timeout { get; set; }

    public ParameterJson[] Parameters { get; set; }

    public bool EscapeQueryParameters { get; set; }
}