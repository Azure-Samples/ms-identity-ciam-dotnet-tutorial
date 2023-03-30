using Microsoft.EntityFrameworkCore;
using ToDoListApi.Models;

namespace ToDoListApi;

public class ToDoContext : DbContext
{
    public ToDoContext(DbContextOptions<ToDoContext> options) : base(options)
    {
    }

    public DbSet<ToDo>? ToDos { get; set; }
}
