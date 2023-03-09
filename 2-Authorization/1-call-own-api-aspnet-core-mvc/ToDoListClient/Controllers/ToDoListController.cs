using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using ToDoListApi.Models;

namespace ToDoListClient.Controllers;

public class TodoListController : Controller
{
    private IDownstreamApi _downstreamApi;
    private const string ServiceName = "ToDoApi";

    public TodoListController(IDownstreamApi downstreamApi)
    {
        _downstreamApi = downstreamApi;
    }

    public async Task<ActionResult> Index()
    {
        Console.WriteLine(HttpContext.User.GetObjectId());
        var toDos = await _downstreamApi.GetForUserAsync<IEnumerable<ToDo>>(
            ServiceName,
            options => options.RelativePath = "/api/todo");

        return View(toDos);
    }

    public ActionResult Create()
    {
        var toDo = new ToDo() { Message = "" };

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind("Message")] ToDo toDo)
    {
        await _downstreamApi.PostForUserAsync<ToDo, ToDo>(
            ServiceName,
            toDo,
            options => options.RelativePath = "api/todo");

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Edit(int id)
    {
        var toDo = await _downstreamApi.GetForUserAsync<ToDo>(
            ServiceName,
            options => options.RelativePath = $"api/todo/{id}");

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, [Bind("Message")] ToDo toDo)
    {
        await _downstreamApi.PatchForUserAsync<ToDo, ToDo>(
            ServiceName,
            toDo,
            options => options.RelativePath = $"api/todo/{id}");

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Remove(int id)
    {
        var toDo = await _downstreamApi.GetForUserAsync<ToDo>(
            ServiceName,
            options => options.RelativePath = $"api/todo/{id}");

        return View(toDo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, [Bind("ID")] ToDo todo)
    {
        await _downstreamApi.DeleteForUserAsync(
            ServiceName,
            todo,
            options => options.RelativePath = $"api/todo/{id}");

        return RedirectToAction("Index");
    }
}