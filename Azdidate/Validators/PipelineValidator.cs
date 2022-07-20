using Azdidate.DTOs;
using Azdidate.Enums;
using Azdidate.Models;
using Azdidate.Repositories.Abstractions;

namespace Azdidate.Validators;

internal class PipelineValidator
{
    private readonly IPipelineRepository _pipelineRepository;
    
    private const string MessageFormat = "Pipeline with Id {0}: {1}";

    internal PipelineValidator(IPipelineRepository pipelineRepository)
    {
        _pipelineRepository = pipelineRepository;
    }
    
    internal async Task<Result<ValidationResult>> ValidatePipeline(int pipelineId, string? branchName, bool ignoreNonExisting)
    {
        string message;
        var success = true;
        
        var validationResult = await _pipelineRepository.ValidatePipeline(pipelineId, branchName);
        if (validationResult.Object == ValidationStateEnum.Valid)
        {
            message = string.Format(MessageFormat, pipelineId, "valid");
        }
        else if (validationResult.Object == ValidationStateEnum.PipelineNotInBranch)
        {
            message = string.Format(MessageFormat, pipelineId, ignoreNonExisting 
                ? "validation skipped - pipeline not in branch" 
                : "invalid - pipeline not in branch");
            
            if(!ignoreNonExisting)
                success = false;
        }
        else if (validationResult.Object == ValidationStateEnum.Invalid)
        {
            message = string.Format(MessageFormat, pipelineId, $"invalid - {validationResult.ErrorMessage}");
            success = false;
        }
        else
        {
            message = string.Format(MessageFormat, pipelineId, $"error - {validationResult.ErrorMessage}");
            success = false;
        }

        return new Result<ValidationResult>(new ValidationResult(message, success));
    }
}