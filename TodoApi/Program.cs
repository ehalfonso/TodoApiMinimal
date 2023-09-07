using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

RouteGroupBuilder todoitems = app.MapGroup("/todoItems");

todoitems.MapGet("/", GetAllTodos);

todoitems.MapGet("/complete", GetCompleteTodos);

todoitems.MapGet("/{id}", GetTodo);

todoitems.MapPost("/", CreateTodo);

todoitems.MapPut("/{id}", UpdateTodo);

todoitems.MapDelete("/{id}", DeleteTodo);



app.Run();
//https://learn.microsoft.com/es-es/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio

async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x=> new TodoItemDTO(x)).ToListAsync());
}

async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
        ? TypedResults.Ok(new TodoItemDTO(todo))
        : TypedResults.NotFound();
}

async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO=new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NoContent();
}