using System.Net.Http.Headers;
using System.Text;
using Azdidate;
using Azdidate.Repositories;
using Azdidate.Repositories.Abstractions;
using Azdidate.Validators;
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
    var pipelineValidator = new PipelineValidator(pipelineRepository);

    var pipelineIds = await GetPipelineIds(pipelineRepository, arguments.RepositoryName);
    Console.WriteLine($"Validating {pipelineIds.Length} pipelines...");
    
    foreach (var pipelineId in pipelineIds)
    {
        var validationResult = await pipelineValidator.ValidatePipeline(pipelineId, arguments.BranchName, arguments.IgnoreNonExisting);
        
        Console.WriteLine(validationResult.Object is not null
            ? validationResult.Object.LogMessage
            : validationResult.ErrorMessage);

        if((validationResult.Object is not null && !validationResult.Object.Success) || validationResult.Object is null)
            Environment.ExitCode = -1;
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

async Task<int[]> GetPipelineIds(IPipelineRepository pipelineRepository, string repositoryName)
{
    var pipelineIdsResult = await pipelineRepository.GetPipelineIds(repositoryName);
    if (pipelineIdsResult.Object is null)
    {
        LogErrorAndExit(pipelineIdsResult.ErrorMessage);
        return Array.Empty<int>();
    }

    return pipelineIdsResult.Object.ToArray();
}


void LogErrorAndExit(string errorMessage)
{
    Console.WriteLine($"Error: {errorMessage}");
    Environment.Exit(-1);
}