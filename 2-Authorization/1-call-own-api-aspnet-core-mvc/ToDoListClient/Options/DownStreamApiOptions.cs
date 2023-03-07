namespace ToDoListClient.Options;

public class DownstreamApiOptions
{
    public const string DownstreamApi = "DownStreamApi";

    /// <summary>
    /// Base URL of the API being called
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:44372/";

    /// <summary>
    /// Space separated string of scopes to access from the API
    /// </summary>
    public string? Scopes { get; set; }
}