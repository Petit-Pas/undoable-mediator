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

        public TestController(ILogger<TestController> logger, IUndoableMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("getRandomInt")]
        public async Task<ActionResult<int>> GetInt()
        {
            var query = new RandomIntQuery();
            var result = await _mediator.QueryAsync(query);

            return Ok(result.Response);
        }

        [HttpGet]
        [Route("setDefinedInt")]
        public async Task<ActionResult> SetInt()
        {
            var command = new ChangeAgeCommand(25);

            await _mediator.SendAsync(command);

            return Ok();
        }

        [HttpGet]
        [Route("setRandomAge")]
        public async Task<ActionResult<int>> Get()
        {
            var command = new SetRandomAgeCommand();

            var result = await _mediator.SendAsync(command);
            await _mediator.UndoLastCommandAsync();

            await _mediator.RedoLastUndoneCommandAsync();

            return Ok(result.Response);
        }
    }
}