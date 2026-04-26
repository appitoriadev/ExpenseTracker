using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserExpensesController : ControllerBase
{
    private readonly IUserExpenseService _service;

    public UserExpensesController(IUserExpenseService service)
    {
        _service = service;
    }

    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<UserExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var userExpenses = await _service.GetByUserIdAsync(userId);
        return Ok(userExpenses);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userExpense = await _service.GetByIdAsync(id);
        return userExpense is null ? NotFound() : Ok(userExpense);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserExpenseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserExpenseDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
