using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using ToDoListApi.Models;

const string ServiceName = "ToDoApi";

// Get the Token acquirer factory instance. By default it reads an appsettings.json
// file if it exists in the same folder as the app (make sure that the 
// "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

// Configure the application options to be read from the configuration
// and add the services you need (Graph, token cache)
tokenAcquirerFactory.Services.AddDownstreamApi(ServiceName,
    tokenAcquirerFactory.Configuration.GetSection("MyWebApi"));

// By default, you get an in-memory token cache.
// For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

// Resolve the dependency injection.
var serviceProvider = tokenAcquirerFactory.Build();


var toDoApiClient = serviceProvider.GetRequiredService<IDownstreamApi>();

Console.WriteLine("Posting a to-do...\n\n");

// Upload a sample to-do
var firstNewToDo = await toDoApiClient.PostForAppAsync<ToDo, ToDo>(
            ServiceName,
            new ToDo()
            {
                UserId = Guid.NewGuid(),
                Message = "Bake bread"
            });

await DisplayToDosFromServer();

Console.WriteLine("Posting a second to-do...\n\n");

// Upload a sample to-do
var secondNewToDo = await toDoApiClient.PostForAppAsync<ToDo, ToDo>(
            ServiceName,
            new ToDo()
            {
                UserId = Guid.NewGuid(),
                Message = "Butter bread"
            });


await DisplayToDosFromServer();

Console.WriteLine("Deleting a to-do...\n\n");
await toDoApiClient.DeleteForAppAsync(
            ServiceName,
            firstNewToDo,
            options => options.RelativePath = $"api/todo/{firstNewToDo!.ID}");;

await DisplayToDosFromServer();

Console.WriteLine("Editing a to-do...\n\n");

secondNewToDo!.Message = "Eat bread";

secondNewToDo = await toDoApiClient.PatchForAppAsync<ToDo, ToDo>(
            ServiceName,
            secondNewToDo!,
            options => options.RelativePath = $"api/todo/{secondNewToDo!.ID}");;

await DisplayToDosFromServer();

Console.WriteLine("Deleting remaining to-do...\n\n");

await toDoApiClient.DeleteForAppAsync(
            ServiceName,
            secondNewToDo!,
            options => options.RelativePath = $"api/todo/{secondNewToDo!.ID}");;

await DisplayToDosFromServer();

async Task DisplayToDosFromServer()
{
    Console.WriteLine("Retrieving to-do's from server...");
    var toDos = await toDoApiClient!.GetForAppAsync<IEnumerable<ToDo>>(
        ServiceName,
        options => options.RelativePath = "/api/todo"
    );

    Console.WriteLine("To-do data:");

    foreach (var toDo in toDos!) {
        DisplayToDo(toDo);
    }
}

void DisplayToDo(ToDo toDo) {
    Console.WriteLine($"ID: {toDo.ID}");
    Console.WriteLine($"User ID: {toDo.UserId}");
    Console.WriteLine($"Message: {toDo.Message}");
}
