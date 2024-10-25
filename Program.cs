using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Product.Api;
using Store.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:5173") // React app URL
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});


var connString = builder.Configuration.GetConnectionString("ProductNewContext");

builder.Services.AddSqlServer<MyDbContext>(connString);

// Add services to the container, including the database context
/* builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); */

builder.Services.AddControllers().AddJsonOptions(x =>
x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin"); // Use the CORS policy
app.UseAuthorization();


var group = app.MapGroup("/pizzas").WithParameterValidation();

const string itemEndpoint = "getItemById";
const string customerEndpoint = "getCustomerById";

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

// GET all customers info
group.MapGet("/customer", async (MyDbContext dbContext) =>
{
    var customers = await dbContext.Customers.ToListAsync();

    return customers.Count > 0 ? Results.Ok(customers) : Results.NotFound("No customers found.");
});




//Get customer details by id
group.MapGet("/customer/{id}", async (int id, MyDbContext dbContext) =>
{
    // Fetch the customer along with related orders and order details
    var customer = await dbContext.Customers
        .Include(c => c.Orders)                      // Include Orders
        .ThenInclude(o => o.OrderDetails)            // Include OrderDetails within Orders
        .FirstOrDefaultAsync(c => c.CustomerId == id);

    // Check if customer exists
    if (customer == null)
    {
        return Results.NotFound();
    }

    // Return the customer data with orders and order details
    return Results.Ok(customer);
}).WithName(customerEndpoint);

// POST endpoint to create a new customer
group.MapPost("/customer", async (MyDbContext dbContext, Customer newCustomer) =>
{
    // Add the item to the database context
    dbContext.Customers.Add(newCustomer);

    // Save changes to the database
    await dbContext.SaveChangesAsync();

    // Return the created item with a 201 Created response
    return Results.CreatedAtRoute(customerEndpoint, new { id = newCustomer.CustomerId }, newCustomer);
});


//POST request to create new order and order details
group.MapPost("/orders", async (MyDbContext dbContext, Order newOrder) =>
{
    // Step 1: Find if the customer exists
    var customer = await dbContext.Customers.FindAsync(newOrder.CustomerId);
    if (customer == null)
    {
        return Results.NotFound($"Customer with ID {newOrder.CustomerId} not found.");
    }

    // Step 2: Create a new Order
    var order = new Order
    {
        CustomerId = newOrder.CustomerId,
        OrderDateTime = newOrder.OrderDateTime,
        TotalAmount = newOrder.TotalAmount,
        PaymentStatus = newOrder.PaymentStatus,
        DeliveryStatus = newOrder.DeliveryStatus
    };

    // Step 3: Add each OrderDetail from the Order
    foreach (var detailDto in newOrder.OrderDetails)
    {
        var orderDetail = new OrderDetail
        {
            ItemId = detailDto.ItemId,
            Size = detailDto.Size,
            Quantity = detailDto.Quantity,
            PricePerPiece = detailDto.PricePerPiece,
            CustomerId = newOrder.CustomerId,  // CustomerId for OrderDetail
            Order = order // Link the order detail to the order
        };

        // Add the order detail to the order's list of details
        order.OrderDetails.Add(orderDetail);
    }

    // Step 4: Add the new order to the context
    dbContext.Orders.Add(order);

    // Step 5: Save changes to the database
    await dbContext.SaveChangesAsync();

    // Step 6: Return a success response
    return Results.Created($"/orders/{order.OrderId}", order);
});



app.Run();