using Microsoft.AspNetCore.Mvc;
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
        [Route("setRandomAge")]
        public ActionResult<int> Get()
        {
            var command = new SetRandomAgeCommand();

            var result = _mediator.Execute(command);

            return Ok(result?.Response);
        }
    }
}