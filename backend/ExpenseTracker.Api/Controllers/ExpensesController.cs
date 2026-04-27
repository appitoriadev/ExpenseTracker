using System.Security.Claims;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    /// <summary>Returns all expenses, ordered by date descending.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var expenses = await _service.GetAllAsync();
        return Ok(expenses);
    }

    /// <summary>Returns a single expense by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var expense = await _service.GetByIdAsync(id);
        return expense is null ? NotFound() : Ok(expense);
    }

    /// <summary>Creates a new expense.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing expense.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Deletes an expense by ID.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
