using GomokuApi.Mapping;
using GomokuApi.Models;
using GomokuApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GomokuApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController(IGomokuService gomokuService, ILogger<AIController> logger) : ControllerBase {
    private readonly IGomokuService _gomokuService = gomokuService;
    private readonly ILogger<AIController> _logger = logger;

    [HttpPost("move")]
    public ActionResult<MoveModel> GetMove([FromBody] GameStateRequestModel request) {
        try {
            // Map the frontend request to backend model
            GameStateModel gameState = GameStateMapper.MapFromFrontend(request);
            // Get the best move from the AI
            MoveModel? bestMove = _gomokuService.GetBestMove(gameState);
            if (bestMove == null) {
                return BadRequest("No valid moves available");
            }

            return Ok(bestMove);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error calculating AI move");
            return StatusCode(500, "Error calculating move");
        }
    }
}