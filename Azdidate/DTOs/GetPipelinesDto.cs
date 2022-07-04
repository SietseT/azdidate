namespace Azdidate.DTOs;

internal class GetPipelinesDto
{
    public IEnumerable<BuildDefinition>? Value { get; set; }

    internal class BuildDefinition
    {
        public int Id { get; set; }
    }
}