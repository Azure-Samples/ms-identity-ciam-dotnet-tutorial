using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using ToDoListApi.Models;
using ToDoListApi.Options;
using ToDoListApi.Services;

namespace ToDoListApi.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ToDoController : ControllerBase
{
    private readonly IToDoService _toDoService;

    public ToDoController(IToDoService toDoService)
    {
        _toDoService = toDoService;
    }

    [HttpGet()]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> GetAsync()
    {
        var toDos = await _toDoService.GetToDosAsync();
        return Ok(toDos);
    }

    [HttpGet("{id}", Name = "Get")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> GetAsync(int id)
    {
        var toDo = await _toDoService.GetToDoAsync(id);

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
        await _toDoService.DeleteTodoAsync(id);
        return Ok();
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> PostAsync([FromBody] ToDo toDo)
    {
        var newToDo = await _toDoService.CreateToDoAsync(toDo.Message);        
    
        return Created($"/todo/{newToDo!.ID}", newToDo);
    }

    [HttpPatch("{id}")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoWriteClaimsKey,
        RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
    public async Task<IActionResult> PatchAsync(int id, [FromBody] ToDo toDo)
    {
        var editedToDo = await _toDoService.EditToDoAsync(id, toDo.Message);

        if (editedToDo is null)
        {
            return NotFound();
        }

        return Ok(editedToDo);
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