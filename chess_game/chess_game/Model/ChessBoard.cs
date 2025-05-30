﻿using chess_game.Model.ChessPieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_game.Model
{
    public class ChessBoard
    {
        public ChessPiece[,] Board { get; set; } // 8x8 array of pieces
        public string CurrentPlayer { get; private set; } = "White"; // White starts the game
        public event Action<string> CheckmateOccurred;
        public ChessBoard() // the coonstructor to create a 8x8 grid 
        {
            Board = new ChessPiece[8, 8];

        }

        public void InitializeBoard()
        {
            // clear the board
            Board = new ChessPiece[8, 8];

            // place black pawns (row 1)
            for (int col = 0; col < 8; col++)
            {
                Board[1, col] = new Pawn("Black", new Position(1, col));
            }

            // place white pawns (row 6)
            for (int col = 0; col < 8; col++)
            {
                Board[6, col] = new Pawn("White", new Position(6, col));
            }

            // place black rooks
            Board[0, 0] = new Rook("Black", new Position(0, 0));
            Board[0, 7] = new Rook("Black", new Position(0, 7));

            // place white rooks
            Board[7, 0] = new Rook("White", new Position(7, 0));
            Board[7, 7] = new Rook("White", new Position(7, 7));

            // place knights
            Board[0, 1] = new Knight("Black", new Position(0, 1));
            Board[0, 6] = new Knight("Black", new Position(0, 6));
            Board[7, 1] = new Knight("White", new Position(7, 1));
            Board[7, 6] = new Knight("White", new Position(7, 6));

            // place bishops
            Board[0, 2] = new Bishop("Black", new Position(0, 2));
            Board[0, 5] = new Bishop("Black", new Position(0, 5));
            Board[7, 2] = new Bishop("White", new Position(7, 2));
            Board[7, 5] = new Bishop("White", new Position(7, 5));

            // place queens
            Board[0, 3] = new Queen("Black", new Position(0, 3));
            Board[7, 3] = new Queen("White", new Position(7, 3));

            // place kings
            Board[0, 4] = new King("Black", new Position(0, 4));
            Board[7, 4] = new King("White", new Position(7, 4));
        }

        public bool TryMovePiece(Position from, Position to)
        {
            var piece = Board[from.Row, from.Col];
            if (piece == null || piece.Color != CurrentPlayer)
            {
                return false; // Invalid move
            }

            if (piece.IsMoveValid(to, this))
            {
                // Simulate the move
                var originalPiece = Board[to.Row, to.Col];
                Board[to.Row, to.Col] = piece;
                Board[from.Row, from.Col] = null;
                piece.Position = to;

                // Check if the move puts the current player's king in check
                if (IsKingInCheck(CurrentPlayer))
                {
                    // Undo the move
                    Board[from.Row, from.Col] = piece;
                    Board[to.Row, to.Col] = originalPiece;
                    piece.Position = from;
                    return false; // Move is invalid because it leaves the king in check
                }

                // Switch turn
                CurrentPlayer = CurrentPlayer == "White" ? "Black" : "White";

                // Check for checkmate
                if (IsCheckmate(CurrentPlayer))
                {
                    string winner = CurrentPlayer == "White" ? "Black" : "White"; // Determine the correct winner
                    CheckmateOccurred?.Invoke(winner); // Raise the event with the correct winner
                }

                return true;
            }

            return false; // Invalid move
        }
        public bool IsMoveValid(ChessPiece piece, Position newPosition) // a simple method which returns a bool T/F if it's a valid move.
        {
            return piece.IsMoveValid(newPosition, this); 
        }
        public bool IsKingInCheck(string kingColor)
        {
            // Find the king's position
            Position kingPosition = null;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = Board[row, col];
                    if (piece is King && piece.Color == kingColor)
                    {
                        kingPosition = new Position(row, col);
                        break;
                    }
                }
                if (kingPosition != null) break;
            }

            if (kingPosition == null)
            {
                throw new InvalidOperationException($"No {kingColor} king found on the board.");
            }

            // Check if any opponent piece can attack the king
            string opponentColor = kingColor == "White" ? "Black" : "White";
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = Board[row, col];
                    if (piece != null && piece.Color == opponentColor)
                    {
                        if (piece.IsMoveValid(kingPosition, this))
                        {
                            return true; // King is in check
                        }
                    }
                }
            }

            return false; // King is not in check
        }
        public bool IsCheckmate(string kingColor)
        {
            if (!IsKingInCheck(kingColor))
            {
                return false; // Not in check, so not in checkmate
            }

            // Check if any move can remove the check
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var piece = Board[row, col];
                    if (piece != null && piece.Color == kingColor)
                    {
                        var validMoves = piece.GetValidMoves(Board, row, col);
                        foreach (var move in validMoves)
                        {
                            // Simulate the move
                            var originalPiece = Board[move.Item1, move.Item2];
                            Board[move.Item1, move.Item2] = piece;
                            Board[row, col] = null;

                            bool isStillInCheck = IsKingInCheck(kingColor);

                            // Undo the move
                            Board[row, col] = piece;
                            Board[move.Item1, move.Item2] = originalPiece;

                            if (!isStillInCheck)
                            {
                                return false; // Found a move that removes the check
                            }
                        }
                    }
                }
            }

            return true; // No valid moves to remove the check
        }
    }
}
