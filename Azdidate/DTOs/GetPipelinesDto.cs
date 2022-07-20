using Newtonsoft.Json;

namespace Azdidate.DTOs;

internal class GetPipelinesDto
{
    public IEnumerable<BuildDefinition>? Value { get; set; }

    internal class BuildDefinition
    {
        public int Id { get; set; }
        
       public Repository? Repository { get; set; }
    }

    internal class Repository
    {
        public string? Name { get; set; }
    }
}