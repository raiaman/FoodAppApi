using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OrderService.Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAll")]
    //[Authorize]
    public class OrderController : ControllerBase
    {
        private IOrderRepository _oderRepository;
        private readonly IJwtService _jwtService;

        public OrderController(IOrderRepository oderRepository, IJwtService jwtService)
        {
            _oderRepository = oderRepository;
            _jwtService = jwtService;
        }

        [HttpGet("GetOrder")]
        public async Task<ActionResult<IEnumerable<OrderModel>>> Get(string username)
        {
            // Validate the token
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (_jwtService.ValidateToken(token))
            {
                var orders = await _oderRepository.GetOrder(username);
                if (orders.Count == 0)
                {
                    return NoContent();
                }
                else
                {
                    return Ok(orders);
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("AddOrder")]
        public async Task<ActionResult<OrderModel>> Post(OrderModel order)
        {
            // Validate the token
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (_jwtService.ValidateToken(token))
            {
                if (order == null)
                {
                    return BadRequest();
                }
                bool isAdded = await _oderRepository.AddOrder(order);
                if (isAdded)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}