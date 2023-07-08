using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Web.Models;
using Chamran.Deed.Authorization;
using Chamran.Deed.Configuration;
using Chamran.Deed.Web.Helpers.StimulsoftHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SqlConnectionStringBuilder = System.Data.SqlClient.SqlConnectionStringBuilder;

namespace Chamran.Deed.Web.Controllers.StimulsoftControllers;

[Route("/DataAdapters")]
[ApiController]
[AbpMvcAuthorize(AppPermissions.Pages_Administration)]
public class DataAdaptersController : DeedControllerBase
{
    private readonly LogInManager _logInManager;
    private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
    private readonly IAppConfigurationAccessor _appConfigurationAccessor;

    public DataAdaptersController(
        LogInManager logInManager,
        AbpLoginResultTypeHelper abpLoginResultTypeHelper,
        IAppConfigurationAccessor appConfigurationAccessor)
    {
        _logInManager = logInManager;
        _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
        _appConfigurationAccessor = appConfigurationAccessor;
    }


    [HttpGet]
    [DontWrapResult]
    public async Task GetCommand()
    {
        await ProcessRequest();
    }

    [HttpPost]
    [DontWrapResult]
    public async Task PostCommand()
    {
        await ProcessRequest();
    }

    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly Regex serverCertificateRegex = new Regex(@"Trust\s*Server\s*Certificate\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex sslModeRegex = new Regex(@"SSL\s*Mode|SslMode\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private async Task ProcessRequest()
    {
        var result = new Result { Success = true };
        var encodeResult = false;

        try
        {
            string inputText;
            using (var stream = new StreamReader(Request.Body))
            {
                inputText = await stream.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(inputText) && inputText[0] != '{')
            {
                var buffer = Convert.FromBase64String(ROT13(inputText));
                inputText = Encoding.UTF8.GetString(buffer);
                encodeResult = true;
            }

            var command = JsonSerializer.Deserialize<CommandJson>(inputText, jsonOptions);

            if (command.Command == "GetSupportedAdapters")
            {
                result.Success = true;
                result.Types = new string[] { "MySQL", "Firebird", "MS SQL", "PostgreSQL", "Oracle" };
            }
            else
            {
                switch (command.Database)
                {
                    //case "MySQL":
                    //    if (!sslModeRegex.IsMatch(command.ConnectionString))
                    //        command.ConnectionString += (command.ConnectionString.TrimEnd().EndsWith(";") ? "" : ";") + "SslMode=None;";
                    //    result = SQLAdapter.Process(command, new MySqlConnection(command.ConnectionString)); break;
                    //case "Firebird": result = SQLAdapter.Process(command, new FbConnection(command.ConnectionString)); break;
                    case "MS SQL":
                        if (!serverCertificateRegex.IsMatch(command.ConnectionString))
                            command.ConnectionString += (command.ConnectionString.TrimEnd().EndsWith(";") ? "" : ";") + "TrustServerCertificate=true;";
                        //var sqlconbuilder = new SqlConnectionStringBuilder(command.ConnectionString)
                        //{
                        //    ConnectTimeout = 180,
                        //    UserID = "deed",
                        //    Password = "Y@snaSystemY@snaSystem"
                        //};
                        var cn = _appConfigurationAccessor.Configuration[$"ConnectionStrings:{DeedConsts.ConnectionStringName}"];
                        var sqlconbuilder = new SqlConnectionStringBuilder(cn)
                        {
                            ConnectTimeout = 180,
                        };
                        result = SQLAdapter.Process(command, new SqlConnection(sqlconbuilder.ConnectionString));
                        break;
                    //case "PostgreSQL": result = SQLAdapter.Process(command, new NpgsqlConnection(command.ConnectionString)); break;
                    //case "Oracle": result = SQLAdapter.Process(command, new OracleConnection(command.ConnectionString)); break;
                    default: result.Success = false; result.Notice = $"Unknown database type [{command.Database}]"; break;
                }
            }
        }
        catch (Exception e)
        {
            result.Success = false;
            result.Notice = e.Message;
        }

        result.HandlerVersion = "2023.2.8";
        result.CheckVersion = true;

        var contentType = "application/json";
        var resultText = JsonSerializer.Serialize(result, jsonOptions);
        if (encodeResult)
        {
            resultText = ROT13(Convert.ToBase64String(Encoding.UTF8.GetBytes(resultText)));
            contentType = "text/plain";
        }

        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Engaged-Auth-Token");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.ContentType = contentType;
        await Response.WriteAsync(resultText);
        await Response.CompleteAsync();
    }

    private static string ROT13(string input)
    {
        return string.Join("", input.Select(x => char.IsLetter(x) ? (x >= 65 && x <= 77) || (x >= 97 && x <= 109) ? (char)(x + 13) : (char)(x - 13) : x));
    }
}