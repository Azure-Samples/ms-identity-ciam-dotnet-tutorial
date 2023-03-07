
using ToDoListApi.Models;

namespace ToDoListClient.Services;

public interface IToDoListService
{
    Task<IEnumerable<ToDo>> GetAsync();

    Task<ToDo> GetAsync(int id);

    Task DeleteAsync(int id);

    Task<ToDo?> AddAsync(ToDo todo);

    Task<ToDo?> EditAsync(int id, ToDo todo);
}