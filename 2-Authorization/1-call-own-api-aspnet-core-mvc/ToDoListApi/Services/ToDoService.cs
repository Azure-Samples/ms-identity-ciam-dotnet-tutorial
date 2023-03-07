using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using ToDoListApi.Models;

namespace ToDoListApi.Services;

public class ToDoService : IToDoService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ToDoContext _context;

    public ToDoService(IHttpContextAccessor httpContextAccessor, ToDoContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<ToDo?> CreateToDoAsync(string message)
    {
        var newToDo = new ToDo() {
            UserId = GetUserId(),
            Message = message
        };

        await _context.ToDos!.AddAsync(newToDo);
        await _context.SaveChangesAsync();

        return newToDo;
    }

    public async Task DeleteTodoAsync(int toDoId)
    {
        var toDoToDelete = await _context.ToDos!
            .FirstOrDefaultAsync(td => td.ID == toDoId && td.UserId == GetUserId());
        
        if (toDoToDelete is null)
        {
            return;
        }

        _context.ToDos!.Remove(toDoToDelete);

        await _context.SaveChangesAsync();
    }

    public async Task<ToDo?> EditToDoAsync(int toDoId, string message)
    {
        var toDoToEdit = await _context.ToDos!.FirstOrDefaultAsync(td => td.ID == toDoId);

        if (toDoToEdit is null)
        {
            return null;
        }

        toDoToEdit.Message = message;
        _context.ToDos!.Update(toDoToEdit);
        
        await _context.SaveChangesAsync();

        return toDoToEdit;
    }

    public async Task<ToDo?> GetToDoAsync(int toDoId)
    {
        return await _context.ToDos!
            .FirstOrDefaultAsync(td => td.ID == toDoId && td.UserId == GetUserId());
    }

    public async Task<IEnumerable<ToDo>> GetToDosAsync()
    {
        return await _context.ToDos!
            .Where(td => td.UserId == GetUserId())
            .ToListAsync();
    }

    private Guid GetUserId()
    {
        Guid userId;

        if (!Guid.TryParse(_httpContextAccessor.HttpContext!.User.GetObjectId(), out userId))
        {
            throw new Exception("User ID is not valid.");
        }

        return userId;
    }
}