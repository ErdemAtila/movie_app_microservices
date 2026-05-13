using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.APP.Features.Tokens;

namespace Users.API.Controllers
{
    [Route("api/[controller]")] 
    [ApiController] 
    public class TokensController : ControllerBase
    {
        private readonly IMediator _mediator; 
        private readonly IConfiguration _configuration; 

        public TokensController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        [HttpPost] 
        [Route("~/api/[action]")] 
        public async Task<IActionResult> Token(TokenRequest request)
        {
            request.SecurityKey = _configuration["SecurityKey"]; 
            request.Audience = _configuration["Audience"]; 
            request.Issuer = _configuration["Issuer"]; 
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response is not null)
                    return Ok(response);
                return NotFound(_configuration["TokenMessage:NotFound"]); 
            }
            return BadRequest(_configuration["TokenMessage:BadRequest"]); 
        }

        [HttpPost] 
        [Route("~/api/[action]")] 
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            request.SecurityKey = _configuration["SecurityKey"]; 
            request.Audience = _configuration["Audience"]; 
            request.Issuer = _configuration["Issuer"]; 
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response is not null)
                    return Ok(response);
                return NotFound(_configuration["TokenMessage:NotFound"]); 
            }
            return BadRequest(_configuration["TokenMessage:BadRequest"]); 
        }
    }
}
