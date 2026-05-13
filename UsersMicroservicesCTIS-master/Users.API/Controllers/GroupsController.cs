using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.APP.Features.Groups;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get() // Action
        {
            var query = await _mediator.Send(new GroupQueryRequest());
            var list = await query.ToListAsync();
            return Ok(list); // 200
            // NotFound // 404
            // BadRequest // 400
            // InternalServerError // 500
        }

        [HttpGet("{id}")] // api/Groups/20
        public async Task<IActionResult> Get(int id)
        {
            var query = await _mediator.Send(new GroupQueryRequest());
            var item = await query.SingleOrDefaultAsync(q => q.Id == id);
            if (item is null)
                return NotFound(); // 404
            return Ok(item); // 200
        }

        [HttpPost]
        public async Task<IActionResult> Post(GroupCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response.IsSuccessful)
                    return Ok(response);
                return BadRequest(response); // 400
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Put(GroupUpdateRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = await _mediator.Send(request);
                if (response.IsSuccessful)
                    return Ok(response);
                return BadRequest(response); // 400
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")] // api/Groups/17: route
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new GroupDeleteRequest { Id = id });
            if (response.IsSuccessful)
                return NoContent(); // 204
            return BadRequest(response); // 400
        }
    }
}
