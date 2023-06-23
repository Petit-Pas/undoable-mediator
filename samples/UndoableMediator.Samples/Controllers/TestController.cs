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
        private readonly IServiceProvider _serviceProvider;

        public TestController(ILogger<TestController> logger, IUndoableMediator mediator,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("getRandomInt")]
        public async Task<ActionResult<int>> GetInt()
        {
            var command = new RandomIntQuery();
            var result = await _mediator.Execute(command);

            return Ok(result.Response);
        }

        [HttpGet]
        [Route("setDefinedInt")]
        public ActionResult SetInt()
        {
            var command = new ChangeAgeCommand(25);

            _mediator.Execute(command);

            return Ok();
        }

        [HttpGet]
        [Route("setRandomAge")]
        public async Task<ActionResult<int>> Get()
        {
            var command = new SetRandomAgeCommand();

            var result = await _mediator.Execute(command, IUndoableMediator.AddAlways);
            _mediator.UndoLastCommand();

            _mediator.RedoLastUndoneCommand();

            return Ok(result.Response);
        }
    }
}