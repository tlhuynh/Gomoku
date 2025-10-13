namespace GomokuApi.Models;

public class GameState {
    public int[,] Board { get; set; } = new int[15, 15];
    public int CurrentPlayer { get; set; } = 1;
    public bool IsGameOver { get; set; }
    public int? Winner { get; set; }
}

public class Move {
    public int Row { get; set; }
    public int Col { get; set; }
    public int Player { get; set; }
}

public class MoveResponse {
    public Move Move { get; set; }
    public int Score { get; set; }
}