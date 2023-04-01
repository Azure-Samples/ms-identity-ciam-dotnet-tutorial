using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using ToDoListApi.Models;

const string ServiceName = "ToDoApi";

var publicClientApplication = PublicClientApplicationBuilder
    .Create("bc862355-65d0-4f7c-a45e-df6499781b25")
    .WithAuthority("https://login.microsoftonline.com/afa75af5-3425-40e6-a8e5-d64187afed4a")
    .WithRedirectUri("http://localhost:64170")
    .Build();


var authenticationResult = await publicClientApplication
    .AcquireTokenInteractive(new string[] { "api://9414f642-f677-449d-9102-eae5f649ed1b/ToDoList.Read", "api://9414f642-f677-449d-9102-eae5f649ed1b/ToDoList.Read" })
    .ExecuteAsync();

var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://localhost:44351");

httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

Console.WriteLine("Creating first new to-do...");

var firstxNewToDo = await CreateNewToDo("Bake bread.");

await DisplayToDosFromServerAsync();

Console.WriteLine("Creating second new to-do...");

var secondxNewToDo = await CreateNewToDo("Butter bread.");

await DisplayToDosFromServerAsync();

Console.WriteLine("Deleting first to-do...");

await httpClient.DeleteAsync($"api/todo/{firstxNewToDo.ID}");

await DisplayToDosFromServerAsync();

Console.WriteLine("Editing a to-do...");

await EditAToDo(secondxNewToDo.ID, "Eat bread.");

await DisplayToDosFromServerAsync();

Console.WriteLine("Deleting last reamining to-do...");

await httpClient.DeleteAsync($"api/todo/{secondxNewToDo.ID}");

await DisplayToDosFromServerAsync();

async Task DisplayToDosFromServerAsync()
{
    Console.WriteLine("Retrieving to-do's from server...");

    var toDos = DeserializeToDos(await httpClient!.GetStringAsync("/api/todo"));

    if (!toDos.Any())
    {
        Console.WriteLine("There are no to-dos.");
        return;
    }

    Console.WriteLine("To-do data:");

    foreach (var toDo in toDos!)
    {
        DisplayToDo(toDo);
    }
}

void DisplayToDo(ToDo toDo)
{
    Console.WriteLine($"ID: {toDo.ID}");
    Console.WriteLine($"User ID: {toDo.UserId}");
    Console.WriteLine($"Message: {toDo.Message}");
}

async Task<ToDo> CreateNewToDo(string message)
{
    var newToDo = new ToDo()
    {
        Message = message
    };

    var response = await httpClient!.PostAsync("/api/todo", SerializeToDo(newToDo));

    return DeserializeToDo(await response.Content.ReadAsStringAsync());
}

async Task<ToDo> EditAToDo(int toDoId, string message)
{
    var newToDo = new ToDo()
    {
        Message = message
    };

    var response = await httpClient!.PatchAsync($"/api/todo/{toDoId}", SerializeToDo(newToDo));

    return DeserializeToDo(await response.Content.ReadAsStringAsync());
}

StringContent SerializeToDo(ToDo toDo)
{
    var jsonRequest = JsonSerializer.Serialize(toDo);
    return new StringContent(jsonRequest, Encoding.UTF8, "application/json");
}

IEnumerable<ToDo> DeserializeToDos(string toDos)
{
    return JsonSerializer.Deserialize<IEnumerable<ToDo>>(toDos, new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    })!;
}

ToDo DeserializeToDo(string toDo)
{
    return JsonSerializer.Deserialize<ToDo>(toDo, new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    })!;
}
