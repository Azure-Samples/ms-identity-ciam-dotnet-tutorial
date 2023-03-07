using Microsoft.AspNetCore.Mvc;
using ToDoListApi.Models;
using ToDoListClient.Services;

namespace ToDoListClient.Controllers;

public class TodoListController : Controller
{
    private IToDoListService _todoListService;

    public TodoListController(IToDoListService todoListService)
    {
        _todoListService = todoListService;
    }

    public async Task<ActionResult> Index()
    {
        var result = await _todoListService.GetAsync();
        return View(result);
    }

    // GET: TodoList/Details/5
    public async Task<ActionResult> Details(int id)
    {
        return View(await _todoListService.GetAsync(id));
    }

    // GET: TodoList/Create
    public ActionResult Create()
    {
        ToDo todo = new ToDo() { Message = "" };
        return View(todo);
    }

    // POST: TodoList/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind("Message")] ToDo todo)
    {
        await _todoListService.AddAsync(todo);
        return RedirectToAction("Index");
    }

    // GET: TodoList/Edit/5
    public async Task<ActionResult> Edit(int id)
    {
        ToDo todo = await this._todoListService.GetAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        return View(todo);
    }

    // POST: TodoList/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, [Bind("Message")] ToDo todo)
    {
        await _todoListService.EditAsync(id, todo);
        return RedirectToAction("Index");
    }

    // GET: TodoList/Delete/5
    public async Task<ActionResult> Remove(int id)
    {
        var toDo = await _todoListService.GetAsync(id);

        return View(toDo);
    }

    // POST: TodoList/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, [Bind("ID")] ToDo todo)
    {
        await _todoListService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}