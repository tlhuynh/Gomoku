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

// Contains the move's coordinate from the AI
export interface AIResponse {
  row: number;
  col: number;
}

// Contains information about the state of a cell
export interface GomokuCellState {
  row: number;
  col: number;
  value: string;
  onClick: (row: number, col: number) => void; // ref: https://stackoverflow.com/questions/57510552/react-prop-types-with-typescript-how-to-have-a-function-type?newreg=be1d7e293449446db636a5066a653138
  disabled: boolean;
  isLastMove: boolean;
}

// Contains information about the current game difficulty
export type Difficulty = 'easy' | 'medium' | 'hard';