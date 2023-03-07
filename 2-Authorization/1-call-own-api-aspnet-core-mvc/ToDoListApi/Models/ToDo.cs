namespace ToDoListApi.Models;

public class ToDo
{
    public int ID { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}
