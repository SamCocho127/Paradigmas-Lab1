using Newtonsoft.Json;

namespace HackerRank1.API.DTO;

public class LibraryForm
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }
}
