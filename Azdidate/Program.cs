using System.Net.Http.Headers;
using System.Text;
using Azdidate;
using Azdidate.Enums;
using Azdidate.Repositories;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var parser = Parser.Default;
var parserResult = parser.ParseArguments<Arguments>(args);

if (parserResult.Tag == ParserResultType.Parsed)
    await parserResult.WithParsedAsync(RunValidation);
else
    await parserResult.WithNotParsedAsync(_ => Task.CompletedTask);

async Task RunValidation(Arguments arguments)
{
    var accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN")?.Trim();
    if (string.IsNullOrWhiteSpace(accessToken))
    {
        Console.WriteLine("Missing environment variable: ACCESS_TOKEN");
        Environment.Exit(-1);
    }

    var httpClient = SetupHttpClientAndJsonSettings(arguments.Organisation, arguments.ProjectName, accessToken);
    var pipelineRepository = new PipelineRepository(httpClient);
    var pipelineIds = Array.Empty<int>();

    if (arguments.PipelineId is null)
    {
        var pipelinesResult = await pipelineRepository.GetPipelines();
        if (pipelinesResult.Object == null)
        {
            ErrorWithNonZeroExitCode(pipelinesResult.ErrorMessage);
        }
        else
        {
            pipelineIds = pipelinesResult.Object.Value?.Select(p => p.Id).ToArray() ?? Array.Empty<int>();
        }
    }
    else
    {
        pipelineIds = new[] {arguments.PipelineId.Value};
    }

    Console.WriteLine($"Validating {pipelineIds.Length} pipelines...");

    foreach (var pipelineId in pipelineIds)
    {
        await ValidatePipeline(pipelineId);
    }
    
    async Task ValidatePipeline(int pipelineId)
    {
        var pipelineValidation = await pipelineRepository.ValidatePipeline(pipelineId, arguments.BranchName);

        if (pipelineValidation.Object == ValidationStateEnum.Valid)
        {
            Console.WriteLine($"[VALID]    - Pipeline Id {pipelineId}");
        }
        else if (pipelineValidation.Object == ValidationStateEnum.PipelineNotInBranch)
        {
            Console.WriteLine(
                $"[{(arguments.IgnoreNonExisting ? "SKIP" : "ERROR")}]     - Pipeline Id {pipelineId}: {pipelineValidation.ErrorMessage}");
            if (!arguments.IgnoreNonExisting)
                Environment.ExitCode = -1;
        }
        else if (pipelineValidation.Object == ValidationStateEnum.Invalid)
        {
            Console.WriteLine($"[INVALID]  - Pipeline Id {pipelineId}: ${pipelineValidation.ErrorMessage}");
            Environment.ExitCode = -1;
        }
        else
        {
            Console.WriteLine($"[ERROR]    - Pipeline Id {pipelineId}: ${pipelineValidation.ErrorMessage}");
            Environment.ExitCode = -1;
        }
    }
}

HttpClient SetupHttpClientAndJsonSettings(string organisation, string projectName, string accessToken)
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"PAT:{accessToken}")));

    httpClient.BaseAddress = new Uri($"https://dev.azure.com/{organisation}/{projectName}/");

    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    return httpClient;
}

void ErrorWithNonZeroExitCode(string errorMessage)
{
    Console.WriteLine($"[ERROR] - {errorMessage}");
    Environment.Exit(-1);
}