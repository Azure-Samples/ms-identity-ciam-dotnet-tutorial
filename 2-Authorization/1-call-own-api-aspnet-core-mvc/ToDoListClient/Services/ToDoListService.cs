using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using ToDoListApi.Models;
using ToDoListClient.Options;

namespace ToDoListClient.Services;

public class ToDoListService : IToDoListService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private DownstreamApiOptions _downStreamApiOptions;

    public ToDoListService(HttpClient httpClient, ITokenAcquisition tokenAcquisition, IOptions<DownstreamApiOptions> downStreamApiOption)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _downStreamApiOptions = downStreamApiOption.Value;
    }

    public async Task<ToDo?> AddAsync(ToDo toDo)
    {
        await SetAuthenticationHeader();

        var toDoJson = JsonSerializer.Serialize(toDo);
        var messageContent = new StringContent(toDoJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/api/todo", messageContent);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ToDo>(content);
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task DeleteAsync(int id)
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.DeleteAsync($"/api/todo/{id}");
    }

    public async Task<ToDo?> EditAsync(int id, ToDo toDo)
    {
        await SetAuthenticationHeader();

        var toDoJson = JsonSerializer.Serialize(toDo);
        var messageContent = new StringContent(toDoJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"/api/todo/{id}", messageContent);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ToDo>(content);
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<IEnumerable<ToDo>> GetAsync()
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.GetAsync("/api/todo");

        if (response.IsSuccessStatusCode)
        {
            var todos = await response.Content.ReadFromJsonAsync<IEnumerable<ToDo>>();

            return todos!;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<ToDo> GetAsync(int id)
    {
        
        await SetAuthenticationHeader();

        var response = await _httpClient.GetAsync($"/api/todo/{id}");

        if (response.IsSuccessStatusCode)
        {
            var todo = await response.Content.ReadFromJsonAsync<ToDo>();

            return todo!;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    private async Task SetAuthenticationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
                await _tokenAcquisition.GetAccessTokenForUserAsync(_downStreamApiOptions.Scopes!.Split(' ')));
    }
}
