using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using ToDoListClient.Models;

const string ServiceName = "DownstreamApi";

// Get the Token acquirer factory instance. By default it reads an appsettings.json
// file if it exists in the same folder as the app (make sure that the 
// "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

// Configure the application options to be read from the configuration
// and add the services you need (Graph, token cache)
tokenAcquirerFactory.Services.AddDownstreamApi(ServiceName,
    tokenAcquirerFactory.Configuration.GetSection("DownstreamApi"));

// By default, you get an in-memory token cache.
// For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

// Resolve the dependency injection.
var serviceProvider = tokenAcquirerFactory.Build();


var toDoApiClient = serviceProvider.GetRequiredService<IDownstreamApi>();

Console.WriteLine("Posting a to-do...");

// Upload a sample to-do
var firstNewToDo = await toDoApiClient.PostForAppAsync<ToDo, ToDo>(
            ServiceName,
            new ToDo()
            {
                Owner = Guid.NewGuid(),
                Description = "Bake bread"
            });

await DisplayToDosFromServer();

Console.WriteLine("Posting a second to-do...");

// Upload a sample to-do
var secondNewToDo = await toDoApiClient.PostForAppAsync<ToDo, ToDo>(
            ServiceName,
            new ToDo()
            {
                Owner = Guid.NewGuid(),
                Description = "Butter bread"
            });


await DisplayToDosFromServer();

Console.WriteLine("Deleting a to-do...");
await toDoApiClient.DeleteForAppAsync(
            ServiceName,
            firstNewToDo,
            options => options.RelativePath = $"api/todolist/{firstNewToDo!.Id}");;

await DisplayToDosFromServer();

Console.WriteLine("Editing a to-do...");

secondNewToDo!.Description = "Eat bread";

secondNewToDo = await toDoApiClient.PutForAppAsync<ToDo, ToDo>(
            ServiceName,
            secondNewToDo!,
            options => options.RelativePath = $"api/todolist/{secondNewToDo!.Id}");;

await DisplayToDosFromServer();

Console.WriteLine("Deleting remaining to-do...");

await toDoApiClient.DeleteForAppAsync(
            ServiceName,
            secondNewToDo!,
            options => options.RelativePath = $"api/todolist/{secondNewToDo!.Id}");;

await DisplayToDosFromServer();

async Task DisplayToDosFromServer()
{
    Console.WriteLine("Retrieving to-do's from server...");
    var toDos = await toDoApiClient!.GetForAppAsync<IEnumerable<ToDo>>(
        ServiceName,
        options => options.RelativePath = "/api/todolist"
    );

    if (!toDos!.Any())
    {
        Console.WriteLine("There are no to-do's in server");
        return;
    }

    Console.WriteLine("To-do data:");

    foreach (var toDo in toDos!) {
        DisplayToDo(toDo);
    }
}

void DisplayToDo(ToDo toDo) {
    Console.WriteLine($"ID: {toDo.Id}");
    Console.WriteLine($"User ID: {toDo.Owner}");
    Console.WriteLine($"Message: {toDo.Description}");
}
