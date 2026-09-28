using Microsoft.EntityFrameworkCore;
using CustomerManagement.API.Data;
using CustomerManagement.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CustomerManagement API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// --- CUSTOMER CRUD ENDPOINTS (Connected to Database via EF Core) ---

// GET: Get all customers
app.MapGet("/customers", async (ApplicationDbContext db) =>
    Results.Ok(await db.Customers.ToListAsync()))
   .WithName("GetAllCustomers")
   .WithSummary("Retrieve all customers");

// GET: Get customer by id
app.MapGet("/customers/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
})
.WithName("GetCustomerById")
.WithSummary("Retrieve a customer by ID");

// POST: Create a new customer
app.MapPost("/customers", async (Customer customer, ApplicationDbContext db) =>
{
    customer.Id = 0; // Let database generate identity Id
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{customer.Id}", customer);
})
.WithName("CreateCustomer")
.WithSummary("Create a new customer");

// PUT: Update an existing customer
app.MapPut("/customers/{id:int}", async (int id, Customer updatedCustomer, ApplicationDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) return Results.NotFound();

    customer.Name = updatedCustomer.Name;
    customer.Email = updatedCustomer.Email;
    customer.Phone = updatedCustomer.Phone;

    await db.SaveChangesAsync();
    return Results.Ok(customer);
})
.WithName("UpdateCustomer")
.WithSummary("Update an existing customer");

// DELETE: Delete a customer
app.MapDelete("/customers/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) return Results.NotFound();

    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteCustomer")
.WithSummary("Delete a customer by ID");

app.Run();


