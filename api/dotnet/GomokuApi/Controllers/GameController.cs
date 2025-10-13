using Microsoft.AspNetCore.Mvc;
using GomokuApi.Models;
using GomokuApi.Services;

namespace GomokuApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGomokuService _gomokuService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGomokuService gomokuService, ILogger<GameController> logger)
    {
        _gomokuService = gomokuService;
        _logger = logger;
    }

    [HttpPost("ai-move")]
    public ActionResult<Move> GetAIMove([FromBody] GameState gameState)
    {
        try
        {
            var bestMove = _gomokuService.GetBestMove(gameState);
            if (bestMove == null)
                return BadRequest("No valid moves available");

            return Ok(bestMove);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating AI move");
            return StatusCode(500, "Error calculating move");
        }
    }

    [HttpPost("validate-move")]
    public ActionResult<bool> ValidateMove([FromBody] MoveRequest request)
    {
        try
        {
            return Ok(_gomokuService.IsValidMove(request.GameState, request.Move));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating move");
            return StatusCode(500, "Error validating move");
        }
    }

    [HttpPost("check-win")]
    public ActionResult<bool> CheckWin([FromBody] MoveRequest request)
    {
        try
        {
            return Ok(_gomokuService.CheckWin(request.GameState, request.Move));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking win condition");
            return StatusCode(500, "Error checking win condition");
        }
    }
}

public class MoveRequest
{
    public GameState GameState { get; set; }
    public Move Move { get; set; }
}