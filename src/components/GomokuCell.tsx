import type { GomokuCellState } from "../types/interfaces";

function GomokuCell(GomokuCellState: GomokuCellState) {
  return (
    <button
      className={`gomoku-cell ${GomokuCellState.isLastMove ? 'last-move' : ''}`}
      style={{
        cursor: GomokuCellState.disabled ? 'default' : 'pointer'
      }}
      onClick={() => GomokuCellState.onClick(GomokuCellState.row, GomokuCellState.col)}
      disabled={GomokuCellState.disabled}>
      {GomokuCellState.value}
    </button>
  );
};

export default GomokuCell