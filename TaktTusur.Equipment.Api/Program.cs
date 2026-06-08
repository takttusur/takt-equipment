using Microsoft.EntityFrameworkCore;
using TaktTusur.Equipment.Api.Services.Users;
using TaktTusur.Equipment.DataAccess;
using TaktTusur.Equipment.Domain;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("SQLiteConnection");
builder.Services.AddDbContext<EquipmentDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRolesService, UserRolesService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Automatic application of migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EquipmentDbContext>();
        context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при накатывании миграций.");
    }
}

app.Run();