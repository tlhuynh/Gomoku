// Contains the move's coordinate on the board
export interface Move {
  row: number;
  col: number;
}

// Contains information of the current state the game is in
export interface GameState {
  board: number[][];
  difficulty: Difficulty;
  currentPlayer: number;
  gameStatus: 'not_started' | 'playing' | 'human_wins' | 'ai_wins' | 'draw';
  lastMove?: Move;
}

// Contains information about the current game difficulty
export type Difficulty = 'easy' | 'medium' | 'hard';