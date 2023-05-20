using UndoableMediator.DependencyInjection;
using UndoableMediator.TestModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var test = AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName);

var command = new ChangeAgeCommand(12);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureMediator(options =>
{
    options.AssembliesToScan = new[] { typeof(ChangeAgeCommand).Assembly };
    options.ShouldScanAutomatically = false;
});

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

app.Run();
