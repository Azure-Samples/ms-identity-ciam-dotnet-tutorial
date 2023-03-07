using ToDoListApi.Models;

namespace ToDoListApi.Services;

public interface IToDoService {
    public Task<ToDo?> CreateToDoAsync(string message);
    public Task<ToDo?> GetToDoAsync(int toDoId);
    public Task<IEnumerable<ToDo>> GetToDosAsync();
    public Task<ToDo?> EditToDoAsync(int toDoId, string message);
    public Task DeleteTodoAsync(int toDoId);
}