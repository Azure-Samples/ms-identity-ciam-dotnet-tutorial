using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using ToDoListClient.Models;

namespace ToDoListClient.Controllers;

[Authorize]
public class TodoListController : Controller
{
    private IDownstreamApi _downstreamApi;
    private const string ServiceName = "DownstreamApi";

    public TodoListController(IDownstreamApi downstreamApi)
    {
        _downstreamApi = downstreamApi;
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Read")]
    public async Task<ActionResult> Index()
    {
        var toDos = await _downstreamApi.GetForUserAsync<IEnumerable<ToDo>>(
            ServiceName,
            options => options.RelativePath = "api/todolist");

        return View(toDos);
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Write")]
    public ActionResult Create()
    {
        var toDo = new ToDo() { Description = "" };

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Write")]
    public async Task<ActionResult> Create([Bind("Description")] ToDo toDo)
    {
        await _downstreamApi.PostForUserAsync<ToDo, ToDo>(
            ServiceName,
            toDo,
            options => options.RelativePath = "api/todolist");

        return RedirectToAction("Index");
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Write")]
    public async Task<ActionResult> Edit(int id)
    {
        var toDo = await _downstreamApi.GetForUserAsync<ToDo>(
            ServiceName,
            options => options.RelativePath = $"api/todolist/{id}");

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Write")]
    public async Task<ActionResult> Edit(int id, [Bind("Description")] ToDo toDo)
    {
        await _downstreamApi.PutForUserAsync<ToDo, ToDo>(
            ServiceName,
            toDo,
            options => options.RelativePath = $"api/todolist/{id}");

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Remove(int id)
    {
        var toDo = await _downstreamApi.GetForUserAsync<ToDo>(
            ServiceName,
            options => options.RelativePath = $"api/todolist/{id}");

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes:Write")]
    public async Task<ActionResult> Delete(int id, [Bind("Id")] ToDo todo)
    {
        await _downstreamApi.DeleteForUserAsync(
            ServiceName,
            todo,
            options => options.RelativePath = $"api/todolist/{id}");

        return RedirectToAction("Index");
    }
}
