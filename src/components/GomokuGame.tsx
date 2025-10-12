import { useEffect, useState } from "react";
import useGomokuGame from "../hooks/useGomokuGame";
import "../styles/components.css"
import { API_URL, BOARD_SIZE, CELL_SIZE } from "../constants";

function GomokuGame() {
  const [difficulty, setDifficulty] = useState('medium');
  const [firstMovePlayer, setFirstMovePlayer] = useState("1");
  const [gameStarted, setGameStarted] = useState(false)
  const {
    gameState,
    makeMove,
    resetGame,
    isLoading,
    getStatusMessage,
    boardSize
  } = useGomokuGame(BOARD_SIZE, firstMovePlayer, API_URL); // TODO look into removing this if possible, might be related to using constant

  // TODO Seems to work as intended between the start and reset but need to implement the start check
  useEffect(() => {
    if (gameStarted == false) {
      resetGame();
      console.log("game reset");
    }
  }, [gameStarted, resetGame]);

  function isLastMove(row: number, col: number) {
    return gameState.lastMove?.row === row && gameState.lastMove?.col === col;
  };
  

  function getStatusColor(): string {
    switch (gameState.gameStatus) {
      case 'human_wins': return '#4caf50';
      case 'ai_wins': return '#f44336';
      case 'draw': return '#ff9800';
      default: return '#2196f3';
    }
  };

  return (
    <div className={`main-container`}>
      <h1 className={`main-title`}>Gomoku</h1>
      <div className={`center-container`}>
        {/* Game Status */}
        <div className={`status-container`} style={{ backgroundColor: getStatusColor() }}>
          {isLoading ? (
            <div className={`loading-container`}>
              <div className={`loading-animation`}></div>
              AI is thinking...
            </div>
          ) : (
            getStatusMessage()
          )}
        </div>

        {/* Game Board */}
        <div className="gomoku-board" style={{
          width: `${(boardSize - 1) * CELL_SIZE}px`,
          height: `${(boardSize - 1) * CELL_SIZE}px`,
        }}>
          {Array.from({ length: boardSize * boardSize }, (_, i) => {
            const col = i % boardSize;
            const row = Math.floor(i / boardSize);
            return (              
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
            }))}
        </div>
      </div>

      {/* Controls */}
      <div style={{
        display: 'flex',
        gap: '15px',
        alignItems: 'center',
        flexWrap: 'wrap',
        justifyContent: 'center'
      }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '10px'
        }}>
          <label style={{ fontWeight: 'bold', color: '#333' }}>
            First move:
          </label>
          <select
            value={firstMovePlayer}
            onChange={(e) => setFirstMovePlayer(e.target.value)}
            style={{
              padding: '8px 12px',
              borderRadius: '5px',
              border: '2px solid #ddd',
              fontSize: '14px',
              backgroundColor: 'white',
              color: 'black'
            }}>
            <option value="1">Player one</option>
            <option value="2">Player two</option>
          </select>
        </div>

        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '10px'
        }}>
          <label style={{ fontWeight: 'bold', color: '#333' }}>
            Difficulty:
          </label>
          <select
            value={difficulty}
            onChange={(e) => setDifficulty(e.target.value)}
            style={{
              padding: '8px 12px',
              borderRadius: '5px',
              border: '2px solid #ddd',
              fontSize: '14px',
              backgroundColor: 'white',
              color: 'black'
            }}
          >
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
          </select>
        </div>

        {/* TODO update this to show different message based on whether game started or not */}
        <button
          onClick={() => setGameStarted(false)}
          style={{
            padding: '12px 24px',
            fontSize: '16px',
            fontWeight: 'bold',
            color: 'white',
            backgroundColor: '#0b5fdeff',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            transition: 'all 0.3s ease',
            boxShadow: '0 2px 8px rgba(0,0,0,0.2)'
          }}>Reset</button>

        <button
          onClick={() => setGameStarted(true)}
          style={{
            padding: '12px 24px',
            fontSize: '16px',
            fontWeight: 'bold',
            color: 'white',
            backgroundColor: '#4caf50',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            transition: 'all 0.3s ease',
            boxShadow: '0 2px 8px rgba(0,0,0,0.2)'
          }}>Start</button>
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