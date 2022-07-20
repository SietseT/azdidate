using Azdidate.DTOs;
using Azdidate.Enums;

namespace Azdidate.Repositories.Abstractions;

internal interface IPipelineRepository
{
    Task<Result<IEnumerable<int>>> GetPipelineIds(string repositoryName);
    Task<Result<ValidationStateEnum>> ValidatePipeline(int pipelineId, string? branchName = null);
}