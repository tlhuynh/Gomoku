import { useCallback, useState } from 'react';
import type { GameState, Move, Difficulty } from '../types/interfaces';
import { API_URL, BOARD_SIZE } from '../constants';

class GomokuGame {
  private boardSize: number;
  private firstMovePlayer: number;
  private gameDifficulty: Difficulty = "medium"; // Default difficulty
  private abortController: AbortController | null = null;

  private gameState: GameState;
  
  constructor(boardSize: number = BOARD_SIZE, firstMovePlayer = "1", gameDifficulty: Difficulty = 'medium') {
    this.boardSize = boardSize;
    this.firstMovePlayer = +firstMovePlayer;
    this.gameDifficulty = gameDifficulty;

    this.gameState = this.initializeGame();
  }

  private initializeGame(): GameState {
    const board = Array(this.boardSize)
      .fill(null)
      .map(() => Array(this.boardSize).fill(0));

    return {
      board,
      currentPlayer: this.firstMovePlayer,
      difficulty: this.gameDifficulty,
      gameStatus: 'not_started'
    };
  }

  // Make a human move
  makeHumanMove(row: number, col: number): boolean {
    if (!this.isValidMove(row, col)) {
      return false;
    }

    // Make human move
    this.gameState.board[row][col] = 1;
    this.gameState.lastMove = { row, col };
    
    // Check for win
    if (this.checkWinner(1)) {
      this.gameState.gameStatus = 'human_wins';
      return true;
    }
    
    if (this.isBoardFull()) {
      this.gameState.gameStatus = 'draw';
      return true;
    }

    // Switch to AI turn
    this.gameState.currentPlayer = 2;
    
    return true;
  }

  // Get AI move from backend
  async makeAIMove(difficulty: Difficulty = 'medium'): Promise<void> {
    try {
      // Cancel any previous request
      if (this.abortController) {
        this.abortController.abort();
      }
      
      // Create a new abort controller
      this.abortController = new AbortController();
      
      const response = await fetch(`${API_URL}/game/ai-move`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          board: this.gameState.board,
          difficulty: difficulty
        }),
        signal: this.abortController.signal
      });
      
      // Request completed, clear the controller
      this.abortController = null;

      if (!response.ok) {
        throw new Error('Failed to get AI move');
      }
      
      const aiMove: Move = await response.json();

      // Make AI move
      this.gameState.board[aiMove.row][aiMove.col] = 2;
      this.gameState.lastMove = { row: aiMove.row, col: aiMove.col };
      
      // Check for AI win
      if (this.checkWinner(2)) {
        this.gameState.gameStatus = 'ai_wins';
      } else if (this.isBoardFull()) {
        this.gameState.gameStatus = 'draw';
      } else {
        // Switch back to human
        this.gameState.currentPlayer = 1;
      }
      
    } catch (error) {
      // Check if this was an abort error (which is expected during cancellation)
      if (error instanceof DOMException && error.name === 'AbortError') {
        console.log('AI move request was cancelled');
        return; // Don't do fallback for intentional cancellation
      }
      
      console.error('Error getting AI move:', error);
      // Fallback to random move if API fails for other reasons
      this.makeRandomAIMove();
    }
  }

  // Fallback random AI move
  private makeRandomAIMove(): void {
    const availableMoves: Move[] = [];
    
    for (let row = 0; row < this.boardSize; row++) {
      for (let col = 0; col < this.boardSize; col++) {
        if (this.gameState.board[row][col] === 0) {
          availableMoves.push({ row, col });
        }
      }
    }
    
    if (availableMoves.length > 0) {
      const randomMove = availableMoves[Math.floor(Math.random() * availableMoves.length)];
      this.gameState.board[randomMove.row][randomMove.col] = 2;
      this.gameState.lastMove = randomMove;
      this.gameState.currentPlayer = 1;
    }
  }

  private isValidMove(row: number, col: number): boolean {
    return (
      row >= 0 && row < this.boardSize &&
      col >= 0 && col < this.boardSize &&
      this.gameState.board[row][col] === 0 &&
      this.gameState.gameStatus === 'playing' &&
      this.gameState.currentPlayer === 1
    );
  }

  private checkWinner(player: number): boolean {
    const directions = [[0, 1], [1, 0], [1, 1], [1, -1]];
    
    for (let row = 0; row < this.boardSize; row++) {
      for (let col = 0; col < this.boardSize; col++) {
        if (this.gameState.board[row][col] === player) {
          for (const [dr, dc] of directions) {
            if (this.checkDirection(row, col, dr, dc, player)) {
              return true;
            }
          }
        }
      }
    }
    return false;
  }

  private checkDirection(startRow: number, startCol: number, dr: number, dc: number, player: number): boolean {
    let count = 0;
    
    for (let i = 0; i < 5; i++) {
      const row = startRow + i * dr;
      const col = startCol + i * dc;
      
      if (
        row >= 0 && row < this.boardSize &&
        col >= 0 && col < this.boardSize &&
        this.gameState.board[row][col] === player
      ) {
        count++;
      } else {
        break;
      }
    }
    
    return count === 5;
  }

  private isBoardFull(): boolean {
    for (let row = 0; row < this.boardSize; row++) {
      for (let col = 0; col < this.boardSize; col++) {
        if (this.gameState.board[row][col] === 0) {
          return false;
        }
      }
    }
    return true;
  }

  // Public getters
  getGameState(): GameState {
    return { ...this.gameState };
  }

  getBoardSize(): number {
    return this.boardSize;
  } 

  // Add method to cancel any pending request
  cancelPendingRequests(): void {
    if (this.abortController) {
      this.abortController.abort();
      this.abortController = null;
    }
  }

  resetGame(firstMovePlayer: number): void {
    // Cancel any pending AI moves before resetting
    this.cancelPendingRequests();
    
    this.firstMovePlayer = firstMovePlayer;
    this.gameState = this.initializeGame();
  }

  startGame(firstMovePlayer: number): void {
    if (this.gameState.gameStatus === 'not_started') {
      this.gameState.gameStatus = 'playing';
      this.gameState.currentPlayer = firstMovePlayer;
    }
  }

  // Check if it's human's turn
  isHumanTurn(): boolean {
    return this.gameState.currentPlayer === 1 && this.gameState.gameStatus === 'playing';
  }

  // Get game status message
  getStatusMessage(): string {
    switch (this.gameState.gameStatus) {
      case 'human_wins': return 'You win! 🎉';
      case 'ai_wins': return 'AI wins! 🤖'; // TODO update this when implementing human player 2
      case 'draw': return "It's a draw! 🤝";
      case 'playing':
        return this.gameState.currentPlayer === 1 
          ? 'Player 1 turn' 
          : 'AI is thinking...'; // TODO update this when implementing human player 2
      case 'not_started': return 'Please select who goes first and start the game.';
      default: return '';
    }
  }
}

// Custom Hook for Gomoku Game
const useGomokuGame = (boardSize: number = BOARD_SIZE, firstMovePlayer = "1") => {
  const [game] = useState(() => new GomokuGame(boardSize, firstMovePlayer));
  const [gameState, setGameState] = useState(game.getGameState());
  const [isLoading, setIsLoading] = useState(false);

  const makeMove = useCallback(async (row: number, col: number) => {
    if (!game.isHumanTurn() || isLoading) return;

    setIsLoading(true);
    // Handle human move first
    const success: boolean = game.makeHumanMove(row, col);
    if (success) {
      setGameState(game.getGameState());
      
      // Check if game is still in progress before making AI move
      const currentState = game.getGameState();
      if (currentState.gameStatus === 'playing' && currentState.currentPlayer === 2) {
        // Now handle AI move separately
        await game.makeAIMove(currentState.difficulty);
        setGameState(game.getGameState());
      }
    }
    setIsLoading(false);
  }, [game, isLoading]);

  const resetGame = useCallback((firstMovePlayer: number) => {
    game.resetGame(firstMovePlayer);
    setGameState(game.getGameState());
    setIsLoading(false);
  }, [game]);

  const startGame = useCallback(async (firstMovePlayer: number) => {
    // Set the first move player and start the game
    game.startGame(firstMovePlayer);

    if (firstMovePlayer === 2) { // TODO update this when implementing human player 2
      // AI goes first
      setIsLoading(true);
      await game.makeAIMove().then(() => {
        setGameState(game.getGameState());
        setIsLoading(false);
      });
    } else {
      // Human goes first
      setGameState(game.getGameState());
    }
  }, [game]);

  return {
    gameState,
    makeMove,
    resetGame,
    startGame,
    isLoading,
    getStatusMessage: () => game.getStatusMessage(),
    boardSize: game.getBoardSize()
  };
};

export default useGomokuGame