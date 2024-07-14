using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        List<int> dX = new List<int> { -1, -1, -1, 0, 0, 1, 1, 1 };
        List<int> dY = new List<int> { -1, 0, 1, -1, 1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newX = currentX + dX[i];
            int newY = currentY + dY[i];

            if (newX >= 0 && newX < tileCountX && newY >= 0 && newY < tileCountY)
            {
                if(board[newX,newY] == null)
                {
                    r.Add(new Vector2Int(newX, newY));
                }

                if(board[newX, newY] != null && board[newX, newY].team != team)
                {
                    r.Add(new Vector2Int(newX, newY));
                }
            }
        }

        return r;
    }

    public override List<Vector2Int> GetAvailableEnemyMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        List<int> dX = new List<int> { -1, -1, -1, 0, 0, 1, 1, 1 };
        List<int> dY = new List<int> { -1, 0, 1, -1, 1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newX = currentX + dX[i];
            int newY = currentY + dY[i];

            if (newX >= 0 && newX < tileCountX && newY >= 0 && newY < tileCountY)
            {
                if (board[newX, newY] != null && board[newX, newY].team != team)
                {
                    r.Add(new Vector2Int(newX, newY));
                }
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableEmptyMoves, ref List<Vector2Int> availableEnemyMoves)
    {
        SpecialMove r = SpecialMove.None;

        var KingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var LeftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var RightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        if(KingMove == null && currentX == 4)
        {
            //White
            if(team == 0)
            {
                //Left
                if(LeftRook == null)
                {
                    if(board[0,0].type == ChessPieceType.Rook)
                    {
                        if(board[0,0].team == 0)
                        {
                            if(board[3,0] == null)
                                if (board[2, 0] == null)
                                    if (board[1, 0] == null)
                                    {
                                        availableEmptyMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                        }
                    }
                }

                //Right
                if (RightRook == null)
                {
                    if (board[7, 0].type == ChessPieceType.Rook)
                    {
                        if (board[7, 0].team == 0)
                        {
                            if (board[5, 0] == null)
                                    if (board[6, 0] == null)
                                    {
                                        availableEmptyMoves.Add(new Vector2Int(6, 0));
                                        r = SpecialMove.Castling;
                                    }
                        }
                    }
                }
            }
            else
            {
                //Left
                if (LeftRook == null)
                {
                    if (board[0, 7].type == ChessPieceType.Rook)
                    {
                        if (board[0, 7].team == 1)
                        {
                            if (board[3, 7] == null)
                                if (board[2, 7] == null)
                                    if (board[1, 7] == null)
                                    {
                                        availableEmptyMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }
                        }
                    }
                }

                //Right
                if (RightRook == null)
                {
                    if (board[7, 7].type == ChessPieceType.Rook)
                    {
                        if (board[7, 7].team == 1)
                        {
                            if (board[5, 7] == null)
                                if (board[6, 7] == null)
                                {
                                    availableEmptyMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }
                        }
                    }
                }
            }
        }
        return r;
    }
}

