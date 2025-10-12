import { useEffect, useState } from "react";
import useGomokuGame from "../hooks/useGomokuGame";
import "../styles/components.css"
import { BOARD_SIZE, CELL_SIZE } from "../constants";

function GomokuGame() {
  const [difficulty, setDifficulty] = useState('medium');
  const [firstMovePlayer, setFirstMovePlayer] = useState("1");
  const [gameStarted, setGameStarted] = useState(false)
  const {
    gameState,
    makeMove,
    resetGame,
    startGame,
    isLoading,
    getStatusMessage,
    boardSize
  } = useGomokuGame(BOARD_SIZE, firstMovePlayer);

  // Track when gameStarted is set back to false before calling resetGame()
  useEffect(() => {
    if (gameStarted) {
      startGame();
      console.log("game started");
    } else {
      resetGame();
      console.log("game reset");
    }
  }, [gameStarted, resetGame, startGame]);

  // Determine whether the move was the most recent
  function isLastMove(row: number, col: number) {
    return gameState.lastMove?.row === row && gameState.lastMove?.col === col;
  };
  
  // Retrieve the color for a specific game status
  function getStatusColor(): string {
    switch (gameState.gameStatus) {
      case 'human_wins': return '#4caf50';
      case 'ai_wins': return '#f44336';
      case 'draw': return '#ff9800';
      default: return '#2196f3';
    }
  };

  return (
    <div className="main-container">
      <h1 className="main-title">Gomoku</h1>
      <div className="center-container">
        {/* TODO this might need to be updated when human player 2 is implemented */}
        {/* Game Status */}        
        <div className="status-container" style={{ backgroundColor: getStatusColor() }}>
          {isLoading ? (
            <div className="loading-container">
              <div className="loading-animation"></div>
              AI is thinking...
            </div>
          ) : (
            getStatusMessage()
          )}
        </div>
        {/* Game Board */}
        <div style={{ position: 'relative' }}>
          <div className="gomoku-board" style={{ width: `${(boardSize - 1) * CELL_SIZE}px`, height: `${(boardSize - 1) * CELL_SIZE}px` }}>
            {/* Mask to prevent interaction until the game is started */}
            {!gameStarted && <div className="gomoku-board-mask">Press Start to Play</div>}

            {/* Draw the board lines */}
            {Array.from({ length: boardSize * boardSize }, (_, i) => {
              const col = i % boardSize;
              const row = Math.floor(i / boardSize);
              return (
                // TODO check if there is a better way to handle this without using overlaped div              
                <div>
                  <div
                    key={i}
                    className="gomoku-board-intersection"
                    style={{
                      left: `${col * CELL_SIZE}px`,
                      top: `${row * CELL_SIZE}px`,
                    }}
                  />
                  <div
                    key={i}
                    className="gomoku-board-intersection-center"
                    style={{
                      left: `${col * CELL_SIZE}px`,
                      top: `${row * CELL_SIZE}px`,
                    }}
                    onClick={() => makeMove(row, col)}
                  />
                </div>
              );
            })}
            {/* Place stones on the board */}
            {gameState.board.map((row, rowIndex) =>
              row.map((cell, colIndex) => {
                const keyString: string = rowIndex.toString() + "-" + colIndex.toString();
                return (cell != 0) ? <div
                  key={keyString}
                  className={`stone ${(cell == +firstMovePlayer) ? "black" : "white"} ${isLastMove(rowIndex, colIndex) ? "lastmove" : "" }`}
                  style={{
                    left: `${colIndex * CELL_SIZE}px`,
                    top: `${rowIndex * CELL_SIZE}px`,
                  }} /> : null
              })
            )}
          </div>
        </div>
      </div>

      {/* Controls */}
      <div className="controls-container">
        {/* Control to select which player start first */}
        <div className="select-dropdown-container">
          <label className="select-dropdown-label">
            First move:
          </label>
          <select
            className="select-dropdown"
            value={firstMovePlayer}
            onChange={(e) => setFirstMovePlayer(e.target.value)}>
            <option value="1">Player one</option>
            <option value="2">Player two</option>
          </select>
        </div>

        {/* Control to select game difficulty */}
        <div className="select-dropdown-container">
          <label className="select-dropdown-label">
            Difficulty:
          </label>
          <select
            className="select-dropdown"
            value={difficulty}
            onChange={(e) => setDifficulty(e.target.value)}>
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
          </select>
        </div>

        {/* Reset button */}
        <button
          className="control-button reset"          
          onClick={() => setGameStarted(false)}>Reset</button>

        {/* Start button */}
        <button
          className="control-button start"
          disabled={gameStarted}
          style={{ backgroundColor: gameStarted ? '#9e9e9e' : '#4caf50', cursor: gameStarted ? 'not-allowed' : 'pointer' }}
          onClick={() => setGameStarted(true)}>Start</button>
      </div>

      {/* Game Rules */}
      <div style={{
        marginTop: '30px',
        padding: '20px',
        backgroundColor: 'white',
        borderRadius: '10px',
        boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
        maxWidth: '600px',
        textAlign: 'center'
      }}>
        <h3 style={{ color: '#333', marginBottom: '10px' }}>How to Play</h3>
        <p style={{ color: '#666', lineHeight: '1.6' }}>
          Select which player to start first (AI is always player 2).
          First move always use black stone .
          Click start to begin the game.
          Click on any empty intersection to place your stone.
          Get 5 stones in a row (horizontally, vertically, or diagonally) to win!
        </p>
      </div>
    </div>
  );
};

export default GomokuGame