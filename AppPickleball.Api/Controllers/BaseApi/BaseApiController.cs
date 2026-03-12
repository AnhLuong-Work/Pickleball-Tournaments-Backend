using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppPickleball.Api.Controllers.BaseApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<BaseApiController> _logger;

        public BaseApiController(
            IMediator mediator,
            ILogger<BaseApiController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

    }
}
