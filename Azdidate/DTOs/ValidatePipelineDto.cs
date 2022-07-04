namespace Azdidate.DTOs;

internal class ValidatePipelineDto
{
    public ValidatePipelineDto(string? branchName = null)
    {
        if (branchName != null)
        {
            Resources = new Resources
            {
                Repositories = new Repository
                {
                    Self = new Self(branchName)
                }
            };
        }
    }
    
    public bool PreviewRun { get; set; } = true;
    
    public Resources? Resources { get; }
}

internal class Resources
{
    public Repository? Repositories { get; set; } 
}

internal class Repository
{
    public Self? Self { get; set; }
}

internal class Self
{
    public Self(string refName)
    {
        RefName = refName;
    }
    
    public string RefName { get; }
}