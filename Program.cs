using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Product.Api;
using Store.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("ProductContext");

builder.Services.AddSqlServer<MyDbContext>(connString);

// Add services to the container, including the database context
/* builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); */

builder.Services.AddControllers().AddJsonOptions(x =>
x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

var group = app.MapGroup("/pizzas").WithParameterValidation();

const string itemEndpoint = "getItemById";

// GET endpoint to retrieve all items with their related menus
group.MapGet("/items", async (MyDbContext dbContext) =>
{
    var items = await dbContext.Items
        .Include(i => i.Menus)  // Include related menus
        .ToListAsync();
    return Results.Ok(items);
});

// GET endpoint to retrieve all menus with their related items
group.MapGet("/menus", async (MyDbContext dbContext) =>
{
    var categories = await dbContext.Menus
        .Include(c => c.Item)  // Include the related item
        .ToListAsync();
    return Results.Ok(categories);
});

//GET Item by id
/* group.MapGet("/items/{id}", async (int id, MyDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);

    return item is not null ? Results.Ok(item) : Results.NotFound();
}).WithName(itemEndpoint);
 */

//GET Item by id with menus
group.MapGet("/items/{id}", async (int id, MyDbContext dbContext) =>
{
    var item = await dbContext.Items.Include(i => i.Menus).FirstOrDefaultAsync(i => i.ItemId == id);

    return item is not null ? Results.Ok(item) : Results.NotFound();
}).WithName(itemEndpoint);


// POST endpoint to create a new Item
group.MapPost("/items", async (MyDbContext dbContext, Item newItem) =>
{
    // Add the item to the database context
    dbContext.Items.Add(newItem);

    // Save changes to the database
    await dbContext.SaveChangesAsync();

    // Return the created item with a 201 Created response
    return Results.CreatedAtRoute(itemEndpoint, new { id = newItem.ItemId }, newItem);
});


app.Run();