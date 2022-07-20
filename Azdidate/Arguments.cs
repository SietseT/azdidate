using CommandLine;

namespace Azdidate;

internal class Arguments
{
    [Option("organisation", Required = true, 
        HelpText = "Azure DevOps organization.")]
    public string Organisation { get; set; } = null!;
    
    [Option("project", Required = true, 
        HelpText = "Azure DevOps project that contains the pipeline(s) to validate.")]
    public string ProjectName { get; set; } = null!;
    
    [Option("repository", Required = true, 
        HelpText = "Repository where pipelines are located.")]
    public string RepositoryName { get; set; } = null!;
    
    [Option("branch", Required = false, 
        HelpText = "Branch to validate against. Will fallback to default branch if not specified.")]
    public string? BranchName { get; set; }
    
    [Option("ignore-non-existing-yaml", Default = false, Required = false, 
        HelpText = "Whether to return a non-zero exit code when the YAML file of a pipeline does not exist in the branch that is checked against.")]
    public bool IgnoreNonExisting { get; set; }
    
    [Option("pipeline-id", Required = false,
        HelpText = "Id of pipeline to validate. When not given, validates all pipelines in the project.")]
    public int? PipelineId { get; set; }
}