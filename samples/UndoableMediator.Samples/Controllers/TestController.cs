using Microsoft.AspNetCore.Mvc;
using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.TestModels;

namespace UndoableMediator.Samples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IUndoableMediator _mediator;
        private readonly IServiceProvider _serviceCollection;
        private readonly ICommandHandler<SetRandomAgeCommand, int> _commandHandler;
        private readonly IServiceProvider _serviceProvider;

        public TestController(ILogger<TestController> logger, IUndoableMediator mediator,  ICommandHandler<SetRandomAgeCommand, int> commandHandler, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _mediator = mediator;
            _commandHandler = commandHandler;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("setRandomAge")]
        public ActionResult<int> Get()
        {
            var test = _serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand>));

            var command = new SetRandomAgeCommand();

            var value1 = AffectedObject.Age;

            var result = _mediator.Execute<SetRandomAgeCommand, int>(command, (_) => true);

            var value2 = AffectedObject.Age;
            
            _mediator.UndoLastCommand();
            
            var value3 = AffectedObject.Age;

            return Ok(result?.Response);
        }
    }
}