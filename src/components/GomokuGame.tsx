import { useState } from "react";
import useGomokuGame from "../hooks/useGomokuGame";
import GomokuCell from "./GomokuCell";
import "../styles/components.css"

function GomokuGame() {
  const {
    gameState,
    makeMove,
    resetGame,
    isLoading,
    getCellValue,
    getStatusMessage,
    boardSize
  } = useGomokuGame(15, '/api'); // TODO look into removing this if possible, might be related to using constant

  const [difficulty, setDifficulty] = useState('medium');

  // Determines whether 
  function isLastMove(row: number, col: number) {
    return gameState.lastMove?.row === row && gameState.lastMove?.col === col;
  };

  const getStatusColor = () => {
    switch (gameState.gameStatus) {
      case 'human_wins': return '#4caf50';
      case 'ai_wins': return '#f44336';
      case 'draw': return '#ff9800';
      default: return '#2196f3';
    }
  };

  return (
    <div className={`main-container`}>
      <h1 style={{
        color: '#333',
        marginBottom: '20px',
        fontSize: '2.5rem',
        textShadow: '2px 2px 4px rgba(0,0,0,0.3)'
      }}>
        Gomoku
      </h1>

      <div style={{
        backgroundColor: 'white',
        padding: '20px',
        borderRadius: '15px',
        boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
        marginBottom: '20px'
      }}>
        {/* Game Status */}
        <div style={{
          textAlign: 'center',
          marginBottom: '20px',
          padding: '15px',
          backgroundColor: getStatusColor(),
          color: 'white',
          borderRadius: '10px',
          fontSize: '1.2rem',
          fontWeight: 'bold',
          minHeight: '50px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          {isLoading ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
              <div style={{
                width: '20px',
                height: '20px',
                border: '2px solid transparent',
                borderTop: '2px solid white',
                borderRadius: '50%',
                animation: 'spin 1s linear infinite'
              }}></div>
              AI is thinking...
            </div>
          ) : (
            getStatusMessage()
          )}
        </div>

        {/* Game Board */}
        <div className="gomoku-board">
          {Array.from({ length: boardSize * boardSize }, (_, i) => {
            const row = Math.floor(i / boardSize);
            const col = i % boardSize;
            const value = getCellValue(row, col);
            
            return (
              <GomokuCell
                key={i}
                row={row}
                col={col}
                value={value}
                onClick={makeMove}
                disabled={isLoading || !value && gameState.gameStatus !== 'playing'}
                isLastMove={isLastMove(row, col)}
              />
            );
          })}
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
              backgroundColor: 'white'
            }}
          >
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
          </select>
        </div>

        <button
          onClick={resetGame}
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
          }}
          // onMouseOver={(e) => {
          //   e.target.style.backgroundColor = '#45a049';
          //   e.target.style.transform = 'translateY(-2px)';
          // }}
          // onMouseOut={(e) => {
          //   e.target.style.backgroundColor = '#4caf50';
          //   e.target.style.transform = 'translateY(0)';
          // }}
        >
          New Game
        </button>
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
          Click on any empty intersection to place your stone (●). 
          Get 5 stones in a row (horizontally, vertically, or diagonally) to win!
          You play as black stones, AI plays as white stones.
        </p>
      </div>

      <style>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
        
        .gomoku-cell:hover:not(:disabled) {
          background-color: #f5deb3 !important;
          transform: scale(1.05);
        }
        
        .last-move {
          background-color: #ffeb3b !important;
          box-shadow: 0 0 0 2px #ff9800;
        }
      `}</style>
    </div>
  );
};

export default GomokuGame