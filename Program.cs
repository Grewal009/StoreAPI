using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5122",
            ValidAudience = "http://localhost:5173",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyVerySecureAndLongSecretKey12345"))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin"); // Use the CORS policy


// Use authentication
app.UseAuthentication();
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
}).RequireAuthorization();



//Login endpoint
group.MapPost("/login", (Customer login) =>
{
    if (true) // Dummy login check
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, login.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyVerySecureAndLongSecretKey12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "http://localhost:5122",
            audience: "http://localhost:5173",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = tokenString });
    }

    return Results.Unauthorized();
});

// GET all orders info
group.MapGet("/orders", async (MyDbContext dbContext) =>
{
    var orders = await dbContext.Orders.ToListAsync();

    return orders.Count > 0 ? Results.Ok(orders) : Results.NotFound("No order found.");
});

//PUT request to update order by id
group.MapPut("/orders/{id}", async (int id, Order updatedOrder, MyDbContext dbContext) =>
{
    // Retrieve the existing order by orderId
    var existingOrder = await dbContext.Orders.FindAsync(id);

    // Check if the order exists
    if (existingOrder == null)
    {
        return Results.NotFound($"Order with ID {id} not found.");
    }

    // Update the fields
    existingOrder.CustomerId = updatedOrder.CustomerId;
    existingOrder.OrderDateTime = updatedOrder.OrderDateTime;
    existingOrder.TotalAmount = updatedOrder.TotalAmount;
    existingOrder.PaymentStatus = updatedOrder.PaymentStatus;
    existingOrder.DeliveryStatus = updatedOrder.DeliveryStatus;

    // Save changes to the database
    await dbContext.SaveChangesAsync();

    return Results.Ok(existingOrder); // Return the updated order
}).RequireAuthorization();




app.Run();