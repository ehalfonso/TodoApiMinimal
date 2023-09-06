using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoitems = app.MapGroup("/todoItems");

todoitems.MapGet("/", GetAllTodos);

async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.ToArrayAsync());
}

todoitems.MapGet("/complete", GetCompleteTodos);

async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
}

todoitems.MapGet("/{id}", GetTodo);

async Task<IResult> GetTodo(int id,TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound();
}

todoitems.MapPost("/", CreateTodo);

async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}

todoitems.MapPut("/{id}", UpdateTodo);

async Task<IResult> UpdateTodo(int id,Todo inputTodo,TodoDb db)
{
    var todo =await db.Todos.FindAsync(id);
    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

todoitems.MapDelete("/{id}", DeleteTodo);

async Task<IResult> DeleteTodo(int id,TodoDb db)
{
    if(await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    return TypedResults.NoContent();
}

app.Run();
//https://learn.microsoft.com/es-es/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio

