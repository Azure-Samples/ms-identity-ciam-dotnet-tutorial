using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using ToDoListClient.Models;

namespace ToDoListClient.Pages.ToDoPages
{

    public class ToDoListBase : ComponentBase
    {
        [Inject]
        IDownstreamApi DownstreamApi { get; set; }

        [Inject]
        MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; }

        [Inject]
        NavigationManager Navigation { get; set; }
        
        const string ServiceName = "DownstreamApi";
        protected IEnumerable<ToDo> toDoList = new List<ToDo>();
        protected ToDo toDo = new ToDo();

        protected override async Task OnInitializedAsync()
        {
            await GetToDoListService();
        }

        /// <summary>
        /// Gets all todo list items.
        /// </summary>
        /// <returns></returns>
        private async Task GetToDoListService()
        {
            try
            {
                toDoList = (await DownstreamApi.GetForUserAsync<IEnumerable<ToDo>>(
                    ServiceName,
                    options => options.RelativePath = "/api/todolist"))!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                // Process the exception from a user challenge
                ConsentHandler.HandleException(ex);
            }
        }

        /// <summary>
        /// Deletes the selected item then retrieves the todo list.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        protected async Task DeleteItem(int Id)
        {
            await DownstreamApi.DeleteForUserAsync(
                ServiceName,
                toDo,
                options => options.RelativePath = $"api/todolist/{Id}");

            await GetToDoListService();
        }
    }
}