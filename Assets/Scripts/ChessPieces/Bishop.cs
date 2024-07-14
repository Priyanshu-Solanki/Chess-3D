using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Up-Right
        for (int i = currentX + 1, j = currentY + 1; i < tileCountX && j< tileCountY; i++, j++)
        {
                if (board[i, j] == null)
                {
                    r.Add(new Vector2Int(i, j));
                }

                if (board[i, j] != null)
                {
                    if (board[i, j].team != team)
                    {
                        r.Add(new Vector2Int(i, j));
                    }
                    break;
                }
        }
        //Up-Left
        for (int i = currentX - 1, j = currentY + 1; i >= 0 && j < tileCountY; i--, j++)
        {
                if (board[i, j] == null)
                {
                    r.Add(new Vector2Int(i, j));
                }

                if (board[i, j] != null)
                {
                    if (board[i, j].team != team)
                    {
                        r.Add(new Vector2Int(i, j));
                    }
                    break;
                }
        }
        //Down-Right
        for (int i = currentX + 1, j = currentY - 1; i < tileCountX && j >= 0; i++, j--)
        {
                if (board[i, j] == null)
                {
                    r.Add(new Vector2Int(i, j));
                }

                if (board[i, j] != null)
                {
                    if (board[i, j].team != team)
                    {
                        r.Add(new Vector2Int(i, j));
                    }
                    break;
                }
        }
        //Down-Left
        for (int i = currentX - 1, j = currentY - 1; i >= 0 && j >= 0; i--, j--)
        {
                if (board[i, j] == null)
                {
                    r.Add(new Vector2Int(i, j));
                }

                if (board[i, j] != null)
                {
                    if (board[i, j].team != team)
                    {
                        r.Add(new Vector2Int(i, j));
                    }
                    break;
                }
        }
        return r;
    }

    public override List<Vector2Int> GetAvailableEnemyMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Up-Right
        for (int i = currentX + 1, j = currentY + 1; i < tileCountX && j < tileCountY; i++, j++)
        {

            if (board[i, j] != null)
            {
                if (board[i, j].team != team)
                {
                    r.Add(new Vector2Int(i, j));
                }
                break;
            }
        }
        //Up-Left
        for (int i = currentX - 1, j = currentY + 1; i >= 0 && j < tileCountY; i--, j++)
        {

            if (board[i, j] != null)
            {
                if (board[i, j].team != team)
                {
                    r.Add(new Vector2Int(i, j));
                }
                break;
            }
        }
        //Down-Right
        for (int i = currentX + 1, j = currentY - 1; i < tileCountX && j >= 0; i++, j--)
        {

            if (board[i, j] != null)
            {
                if (board[i, j].team != team)
                {
                    r.Add(new Vector2Int(i, j));
                }
                break;
            }
        }
        //Down-Left
        for (int i = currentX - 1, j = currentY - 1; i >= 0 && j >= 0; i--, j--)
        {
            if (board[i, j] != null)
            {
                if (board[i, j].team != team)
                {
                    r.Add(new Vector2Int(i, j));
                }
                break;
            }
        }
        return r;

    }

}
