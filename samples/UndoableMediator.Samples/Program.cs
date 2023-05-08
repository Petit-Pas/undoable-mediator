using UndoableMediator.TestModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var command = new SetRandomAgeCommand();

var mediator = new UndoableMediator.Mediators.UndoableMediator(new string[] { "UndoableMediator" });

var result1 = mediator.Execute(command);

var query = new RandomIntQuery();
var result2 = mediator.Execute<int>(query);

var command2 = new CancelableQuery(true);
var result3 = mediator.Execute(command2);

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

app.Run();
