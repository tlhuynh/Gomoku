import { useCallback, useState } from 'react';
import type { GameState, Move, AIResponse, Difficulty } from '../types/interfaces';
import { API_URL } from '../constants';

class GomokuGame {
  private boardSize: number;
  private firstMovePlayer: number;

  private gameState: GameState;
  private apiUrl: string; // TODO this might be able to use a constant value
  
  constructor(boardSize: number = 15, firstMovePlayer = "1", apiUrl: string = API_URL) {
    this.boardSize = boardSize;
    this.firstMovePlayer = +firstMovePlayer; // TODO make sure to check this for potential failure
    this.apiUrl = apiUrl;
    this.gameState = this.initializeGame();
  }

  private initializeGame(): GameState {
    const board = Array(this.boardSize)
      .fill(null)
      .map(() => Array(this.boardSize).fill(0));
    
    return {
      board,
      currentPlayer: this.firstMovePlayer,
      gameStatus: 'playing' // TODO update this
    };
  }

  // Make a human move
  async makeHumanMove(row: number, col: number): Promise<boolean> {
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
    
    // Get AI move
    await this.makeAIMove();
    
    return true;
  }

  // Get AI move from backend
  private async makeAIMove(difficulty: Difficulty = 'medium'): Promise<void> {
    try {
      const response = await fetch(`${this.apiUrl}/ai-move`, { // TODO make sure the name of this route here matches whatever name we use in the backend
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          board: this.gameState.board,
          difficulty
        })
      });

      if (!response.ok) {
        throw new Error('Failed to get AI move');
      }

      const aiMove: AIResponse = await response.json();
      
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
      console.error('Error getting AI move:', error);
      // Fallback to random move if API fails
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

  resetGame(): void {
    this.gameState = this.initializeGame();
  }

  // TODO might not need this
  // Get cell display value
  // getCellValue(row: number, col: number): string {
  //   const value = this.gameState.board[row][col];
  //   switch (value) {
  //     case 1: return '●'; // Human (black)
  //     case 2: return '○'; // AI (white)
  //     default: return '';
  //   }
  // }

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
      default: return '';
    }
  }
}

// Custom Hook for Gomoku Game
const useGomokuGame = (boardSize: number = 15, firstMovePlayer = "1", apiUrl: string = 'http://localhost:8000') => { // TODO update default api
  const [game] = useState(() => new GomokuGame(boardSize, firstMovePlayer, apiUrl));
  const [gameState, setGameState] = useState(game.getGameState());
  const [isLoading, setIsLoading] = useState(false);

  const makeMove = useCallback(async (row: number, col: number) => {
    if (!game.isHumanTurn() || isLoading) return;
    
    setIsLoading(true);
    const success = await game.makeHumanMove(row, col);
    if (success) {
      setGameState(game.getGameState());
    }
    setIsLoading(false);
  }, [game, isLoading]);

  const resetGame = useCallback(() => {
    game.resetGame();
    setGameState(game.getGameState());
    setIsLoading(false);
  }, [game]);

  // TODO add a useCalllbacl here for starting game

  return {
    gameState,
    makeMove,
    resetGame,
    isLoading,
    getStatusMessage: () => game.getStatusMessage(),
    boardSize: game.getBoardSize()
  };
};

export default useGomokuGame