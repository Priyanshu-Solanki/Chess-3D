using System.Collections.Generic;
using UnityEngine;
public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        List<int> dX = new List<int> { -2,-2,-1,-1,1,1,2,2 };
        List<int> dY = new List<int> { -1, 1, -2, 2,2, -2,1, -1};

        for(int i=0;i<8;i++)
        {
            int newX = currentX + dX[i];
            int newY = currentY + dY[i];

            if(newX >= 0 && newX < tileCountX && newY >=0 && newY < tileCountY)
            {
                if(board[newX,newY] == null)
                r.Add(new Vector2Int(newX, newY));

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

        List<int> dX = new List<int> { -2, -2, -1, -1, 1, 1, 2, 2 };
        List<int> dY = new List<int> { -1, 1, -2, 2, 2, -2, 1, -1 };

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
}
