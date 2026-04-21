using UndoableMediator.DependencyInjection;
using UndoableMediator.TestModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
