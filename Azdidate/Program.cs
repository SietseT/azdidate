using System.Net.Http.Headers;
using System.Text;
using Azdidate;
using Azdidate.Repositories;
using Azdidate.Repositories.Abstractions;
using Azdidate.Validators;
using CommandLine;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var parser = Parser.Default;
var parserResult = parser.ParseArguments<Arguments>(args);
var logger = GetConsoleLogger();

if (parserResult.Tag == ParserResultType.Parsed)
    await parserResult.WithParsedAsync(async (arguments) =>
    {
        try
        {
            await RunValidation(arguments);
        }
        catch (Exception ex)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            logger.LogError(ex.Message);
            Thread.Sleep(50); //Sleep, otherwise the log entry will not be written because of async
            Environment.ExitCode = -1;
        }
    });
else
    await parserResult.WithNotParsedAsync(_ => Task.CompletedTask);

async Task RunValidation(Arguments arguments)
{
    var accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN")?.Trim();
    if (string.IsNullOrWhiteSpace(accessToken))
        throw new ArgumentException("Missing environment variable: ACCESS_TOKEN");

    var httpClient = SetupHttpClientAndJsonSettings(arguments.Organisation, arguments.ProjectName, accessToken);
    var pipelineRepository = new PipelineRepository(httpClient);
    var pipelineValidator = new PipelineValidator(pipelineRepository);

    var pipelineIds = await GetPipelineIds(pipelineRepository, arguments.RepositoryName);
    logger.LogInformation("Validating {Pipelines} pipelines...", pipelineIds.Length);
    
    foreach (var pipelineId in pipelineIds)
    {
        var validationResult = await pipelineValidator.ValidatePipeline(pipelineId, arguments.BranchName, arguments.IgnoreNonExisting);

        if (validationResult.Object is not null && validationResult.Object.Success)
            logger.LogInformation("{Message}", validationResult.Object.LogMessage);
        else
            logger.LogWarning("{Error}", validationResult.ErrorMessage);
        
        if((validationResult.Object is not null && !validationResult.Object.Success) || validationResult.Object is null)
            Environment.ExitCode = -1;
    }
}

async Task<int[]> GetPipelineIds(IPipelineRepository pipelineRepository, string repositoryName)
{
    var pipelineIdsResult = await pipelineRepository.GetPipelineIds(repositoryName);
    if (pipelineIdsResult.Object is null)
    {
        throw new Exception(pipelineIdsResult.ErrorMessage);
    }

    return pipelineIdsResult.Object.ToArray();
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

ILogger GetConsoleLogger()
{
    var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
            });
        }
    );

    return loggerFactory.CreateLogger("Azdidate");
}