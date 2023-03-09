using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using ToDoListApi.Models;
using ToDoListApi.Options;

namespace ToDoListApi.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ToDoController : ControllerBase
{
    private readonly ToDoContext _toDoContext;

    public ToDoController(ToDoContext toDoContext)
    {
        _toDoContext = toDoContext;
    }

    [HttpGet()]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> GetAsync()
    {
        var toDos = await _toDoContext.ToDos!
            .Where(td => RequestCanAccessToDo(td.UserId))
            .ToListAsync();

        return Ok(toDos);
    }

    [HttpGet("{id}", Name = "Get")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> GetAsync(int id)
    {
        var toDo = await _toDoContext.ToDos!
            .FirstOrDefaultAsync(td => RequestCanAccessToDo(td.UserId) && td.ID == id);

        if (toDo is null)
        {
            return NotFound();
        }

        return Ok(toDo);
    }

    [HttpDelete("{id}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var toDoToDelete = await _toDoContext.ToDos!
            .FirstOrDefaultAsync(td => RequestCanAccessToDo(td.UserId) && td.ID == id);
        
        if (toDoToDelete is null)
        {
            return NotFound();
        }

        _toDoContext.ToDos!.Remove(toDoToDelete);

        await _toDoContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> PostAsync([FromBody] ToDo toDo)
    {
        // Only let applications with global to-do access set the user ID or to-do's
        var userIdOfTodo = IsAppMakingRequest() ? toDo.UserId : GetUserId();

        var newToDo = new ToDo() {
            UserId = userIdOfTodo,
            Message = toDo.Message
        };

        await _toDoContext.ToDos!.AddAsync(newToDo);
        await _toDoContext.SaveChangesAsync();   
    
        return Created($"/todo/{newToDo!.ID}", newToDo);
    }

    [HttpPatch("{id}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> PatchAsync(int id, [FromBody] ToDo toDo)
    {
        var storedToDo = await _toDoContext.ToDos!.FirstOrDefaultAsync(td => RequestCanAccessToDo(td.UserId) && td.ID == id);

        if (storedToDo is null)
        {
            return NotFound();
        }

        storedToDo.Message = toDo.Message;
        _toDoContext.ToDos!.Update(storedToDo);
        
        await _toDoContext.SaveChangesAsync();

        return Ok(storedToDo);
    }

    private bool RequestCanAccessToDo(Guid userId)
    {
        return IsAppMakingRequest() || (userId == GetUserId());
    }

    private Guid GetUserId()
    {
        Guid userId;

        if (!Guid.TryParse(HttpContext.User.GetObjectId(), out userId))
        {
            throw new Exception("User ID is not valid.");
        }

        return userId;
    }

    private bool IsAppMakingRequest()
    {
        // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
        //
        // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
        return HttpContext.User
            .Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
    }
}
