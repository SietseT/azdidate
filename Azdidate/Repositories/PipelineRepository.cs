using System.Net;
using System.Text;
using Azdidate.DTOs;
using Azdidate.Enums;
using Azdidate.Repositories.Abstractions;
using Newtonsoft.Json;

namespace Azdidate.Repositories;

internal class PipelineRepository : IPipelineRepository
{
    private readonly HttpClient _httpClient;

    internal PipelineRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<IEnumerable<int>>> GetPipelineIds(string repositoryName)
    {
        var buildDefinitions = new List<GetPipelinesDto.BuildDefinition>();
        var continuationToken = string.Empty;

        while (continuationToken is not null)
        {
            var additionalQueryParameters = string.Empty;
            if (!string.IsNullOrWhiteSpace(continuationToken))
                additionalQueryParameters = $"&continuationToken={continuationToken}";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"_apis/build/definitions?includeAllProperties=true&queryOrder=definitionNameAscending{additionalQueryParameters}&$top=1&api-version=7.1-preview", UriKind.Relative));
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await DeserializeFromResponse(response);
                if (result.Object?.Value is not null)
                    buildDefinitions.AddRange(result.Object.Value);
                
                continuationToken = response.Headers.TryGetValues("x-ms-continuationtoken", out var values) 
                    ? values.First() : null;
            }
            else
            {
                return response.StatusCode == HttpStatusCode.Unauthorized
                    ? new Result<IEnumerable<int>>("Personal Access Token is invalid or does not have permissions.")
                    : new Result<IEnumerable<int>>($"Response returned statuscode: {response.StatusCode}");
            }
        }

        var buildDefinitionsFiltered = buildDefinitions.Where(b =>
            b.Repository?.Name != null &&
            b.Repository.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));

        return new Result<IEnumerable<int>>(buildDefinitionsFiltered.Select(p => p.Id));
    }

    public async Task<Result<ValidationStateEnum>> ValidatePipeline(int pipelineId, string? branchName = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"_apis/pipelines/{pipelineId}/runs?api-version=5.1-preview", UriKind.Relative));
        var requestBody = new ValidatePipelineDto(branchName);
        request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(request);

            var validationResult = ValidationStateEnum.Valid;
            string? message;

            if (response.IsSuccessStatusCode)
            {
                return new Result<ValidationStateEnum>(validationResult);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return await HandleBadRequest(response);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                validationResult = ValidationStateEnum.Error;
                message = "Personal Access Token is invalid or does not have permissions.";
            }
            else
            {
                validationResult = ValidationStateEnum.Error;
                message = $"Response returned statuscode: {response.StatusCode}";
            }

            return new Result<ValidationStateEnum>(validationResult, message);
        }
        catch (Exception ex)
        {
            return new Result<ValidationStateEnum>(ex.Message);
        }
    }

    private async Task<Result<GetPipelinesDto>> DeserializeFromResponse(HttpResponseMessage response)
    {
        try
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<GetPipelinesDto>(responseString);
            if (dto == null)
                throw new Exception("Error while deserialising response.");
            
            return new Result<GetPipelinesDto>(dto);
        }
        catch (Exception ex)
        {
            return new Result<GetPipelinesDto>(ex.Message);
        }

    }

    private async Task<Result<ValidationStateEnum>> HandleBadRequest(HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        if (responseString is null)
            throw new Exception("Response could not be read.");

        var responseDto = JsonConvert.DeserializeObject<ValidateResponseDto>(responseString);
        if(responseDto == null || responseDto.Message == null)
            throw new Exception($"Could not deserialize response: {responseString}");

        if (responseDto.Message.Contains("Unable to resolve the reference"))
            return new Result<ValidationStateEnum>(ValidationStateEnum.PipelineNotInBranch, responseDto.Message);

        return new Result<ValidationStateEnum>(ValidationStateEnum.Error, responseDto.Message);
    }
}