using UndoableMediator.Commands;
using UndoableMediator.TestModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var mediator = new UndoableMediator.Mediators.UndoableMediator(keywords: new[] { "UndoableMediator" });

var command = new SetRandomAgeCommand();

var testCommand = new ChangeAgeCommand(15);
//var handler = new ChangeAgeCommandHandler();
//handler.UndoSubCommands(testCommand, mediator);

//var genericHandler = new ChangeAgeCommandHandler() as ICommandHandler<ChangeAgeCommand>;
//genericHandler.UndoSubCommands(testCommand, mediator);

var result1 = mediator.Execute(command, (_) => true);

var query = new RandomIntQuery();
var result2 = mediator.Execute<int>(query);

var command2 = new CancelableQuery(true);
var result3 = mediator.Execute(command2);

var age = AffectedObject.Age;

mediator.UndoLastCommand();

var age2 = AffectedObject.Age;

mediator.UndoLastCommand();


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
