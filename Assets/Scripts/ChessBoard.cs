using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}
public class ChessBoard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deadSize = 0.8f;
    [SerializeField] private float deadSpacing = 0.5f;
    [SerializeField] private float draggingOffset = 1f;
    [SerializeField] private GameObject winScreen;


    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //Logic
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Vector2Int> availableEnemyMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private const int TILECOUNT_X = 8;
    private const int TILECOUNT_Y = 8;

    private GameObject[,] tiles;

    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;

    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private void Awake()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILECOUNT_X, TILECOUNT_Y);

        SpawnAllPieces();

        PositionAllPieces();
    }

    private void Update()
    {
        if(!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "HighLight", "Enemy")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if(currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentlyDragging != null && Input.GetMouseButtonDown(0))
            {
                Vector2Int prevPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(prevPosition.x, prevPosition.y) + ((chessPieces[prevPosition.x, prevPosition.y].type == ChessPieceType.Pawn) ? new Vector3(0, yOffset * 7, 0) : Vector3.zero));
                }
                else
                {
                    currentlyDragging.SetPosition(GetTileCenter(hitPosition.x, hitPosition.y) + ((chessPieces[hitPosition.x, hitPosition.y].type == ChessPieceType.Pawn) ? new Vector3(0, yOffset * 7, 0) : Vector3.zero));
                }
                currentlyDragging = null;
                RemoveHighLightTiles();
            }
            else if(Input.GetMouseButtonDown(0))
            {
                if(chessPieces[hitPosition.x,hitPosition.y] != null)
                {
                    //is our turn
                    if((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        //get list of available moves
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILECOUNT_X, TILECOUNT_Y);
                        availableEnemyMoves = currentlyDragging.GetAvailableEnemyMoves(ref chessPieces, TILECOUNT_X, TILECOUNT_Y);
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves, ref availableEnemyMoves);

                        PreventCheck();
                        HighLightTiles();
                    }
                }
            }
            
            
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if(currentlyDragging && Input.GetMouseButtonDown(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY) 
                    + ((chessPieces[currentlyDragging.currentX, currentlyDragging.currentY].type == ChessPieceType.Pawn) ? new Vector3(0, yOffset * 7, 0) : Vector3.zero));
                currentlyDragging = null;
                RemoveHighLightTiles();
            }
        }

        if(currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;

            if (horizontalPlane.Raycast(ray, out distance))
               currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * draggingOffset);
        }
    }


    //Generating Tiles
    void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;


        tiles = new GameObject[tileCountX, tileCountY];

        for(int x=0;x<tileCountX;x++)
        {
            for(int y=0;y<tileCountY;y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format(" X :{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset , y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset , (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }


    //Spawn ChessPieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILECOUNT_X, TILECOUNT_Y];

        int whiteTeam = 0;
        int blackTeam = 1;

        //white
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        for(int i=0;i<TILECOUNT_X;i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        //black
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TILECOUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    //Positioning

    private void PositionAllPieces()
    {
        for(int x=0;x<TILECOUNT_X;x++)
        {
            for(int y=0;y<TILECOUNT_Y;y++)
            {
                if(chessPieces[x,y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        //chessPieces[x, y].transform.position = GetTileCenter(x,y);
        chessPieces[x, y].SetPosition(GetTileCenter(x, y) + ((chessPieces[x,y].type == ChessPieceType.Pawn) ? new Vector3(0,yOffset * 7,0) : Vector3.zero), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0,tileSize / 2);
    }

    //Highlight Tiles

    private void HighLightTiles()
    {
        for(int i=0;i<availableMoves.Count;i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("HighLight");
        }
        for (int i = 0; i < availableEnemyMoves.Count; i++)
        {
            tiles[availableEnemyMoves[i].x, availableEnemyMoves[i].y].layer = LayerMask.NameToLayer("Enemy");
        }
    }
    private void RemoveHighLightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        for (int i = 0; i < availableEnemyMoves.Count; i++)
        {
            tiles[availableEnemyMoves[i].x, availableEnemyMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
        availableEnemyMoves.Clear();
    }
    //CheckMate
    private void CheckMate(int winner)
    {
        DisplayWinner(winner);
    }

    private void DisplayWinner(int team)
    {
        winScreen.SetActive(true);
        winScreen.transform.GetChild(team).gameObject.SetActive(true);
    }

    public void OnResetButton()
    {
        //UI
        winScreen.transform.GetChild(0).gameObject.SetActive(true);
        winScreen.transform.GetChild(1).gameObject.SetActive(true);
        winScreen.SetActive(false);

        //field reset
        currentlyDragging = null;
        availableMoves = new List<Vector2Int>();
        availableEnemyMoves = new List<Vector2Int>();
        moveList.Clear();

        //cleaning
        for (int x = 0; x < TILECOUNT_X; x++)
        {
            for (int y = 0; y < TILECOUNT_Y; y++)
            {
                if(chessPieces[x,y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);

                    chessPieces[x, y] = null;
                }
            }
        }

        for(int i=0;i<deadWhites.Count;i++)
        {
            Destroy(deadWhites[i].gameObject);
        }
        for (int i = 0; i < deadBlacks.Count; i++)
        {
            Destroy(deadBlacks[i].gameObject);
        }

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    //Special Moves
    private void ProcessSpecialMove()
    {
        if(specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];

            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.currentX == enemyPawn.currentX)
            {
                if (myPawn.currentY == enemyPawn.currentY + 1 || myPawn.currentY == enemyPawn.currentY - 1)
                {
                    if(enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(enemyPawn.transform.localScale * deadSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize,  yOffset * 10 , -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deadSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(enemyPawn.transform.localScale * deadSize);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffset * 10, 8 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.back * deadSpacing) * deadBlacks.Count);
                    }
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }
        if(specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if(targetPawn.type == ChessPieceType.Pawn)
            {
                if(targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);

                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }

                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);

                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }
        if(specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //left
            if(lastMove[1].x == 2) 
            {
                if(lastMove[1].y == 0) // white
                {
                    chessPieces[3, 0] = chessPieces[0, 0];
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                if(lastMove[1].y == 7) //black
                {
                    chessPieces[3, 7] = chessPieces[0, 7];
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }

            //right
            if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) // white
                {
                    chessPieces[5, 0] = chessPieces[7, 0];
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                if (lastMove[1].y == 7) //black
                {
                    chessPieces[5, 7] = chessPieces[7, 7];
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for(int x=0;x<TILECOUNT_X;x++)
        {
            for(int y=0;y<TILECOUNT_Y;y++)
            {
                if(chessPieces[x,y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyDragging.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }

        SimulateMovesForSinglePiece(currentlyDragging,ref availableMoves, ref availableEnemyMoves, targetKing);
    }

    private void SimulateMovesForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves1, ref List<Vector2Int> moves2, ChessPiece targetKing)
    {
        //save current values to reset after function call
        int actualX = cp.currentX;
        int actualY = cp.currentY;

        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //going through all moves and check if we are in check
        for(int i=0;i<moves1.Count;i++)
        {
            int simX = moves1[i].x;
            int simY = moves1[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            //did we simulate king's move
            if(cp.type == ChessPieceType.King)
            {
                kingPositionThisSim = new Vector2Int(simX, simY);
            }

            //copy the array and not a reference
            ChessPiece[,] simulation = new ChessPiece[TILECOUNT_X,TILECOUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILECOUNT_X; x++)
            {
                for (int y = 0; y < TILECOUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }

                //Simulate that move
                simulation[actualX, actualY] = null;
                cp.currentX = simX;
                cp.currentY = simY;

                simulation[simX, simY] = cp;

                //did any piece got killed in simulation
                var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
                if (deadPiece != null)
                {
                    simAttackingPieces.Remove(deadPiece);
                }
            }
                //get all the simulated attacking pieces move
                List<Vector2Int> simMoves = new List<Vector2Int>();
                for(int a=0;a<simAttackingPieces.Count;a++)
                {
                    var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILECOUNT_X, TILECOUNT_Y);

                    for (int b=0;b<pieceMoves.Count;b++)
                    {
                        simMoves.Add(pieceMoves[b]);
                    }
                }

                //Is the king in trouble
                if(ContainsValidMove(ref simMoves, kingPositionThisSim))
                {
                    movesToRemove.Add(moves1[i]);
                }

                //Restore the actual cp data
                cp.currentX = actualX;
                cp.currentY = actualY;
            

        }


        //Remove from current available moveList
        for(int i=0; i< movesToRemove.Count; i++)
        {
            moves1.Remove(movesToRemove[i]);
            if(moves2.Contains(movesToRemove[i]))
            {
                moves2.Remove(movesToRemove[i]);
            }
        }
    }

    private bool CheckCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();

        ChessPiece targetKing = null;
        for (int x = 0; x < TILECOUNT_X; x++)
        {
            for (int y = 0; y < TILECOUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                         defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        // Is king attacked right now
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for(int i=0;i<attackingPieces.Count;i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILECOUNT_X, TILECOUNT_Y);

            for (int b = 0; b < pieceMoves.Count; b++)
            {
                currentAvailableMoves.Add(pieceMoves[b]);
            }

        }

        //Are we in check rn
        if(ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX,targetKing.currentY)))
        {
            //king is under attack can we move something to help him
            for(int i=0;i<defendingPieces.Count;i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILECOUNT_X, TILECOUNT_Y);

                SimulateMovesForSinglePiece(defendingPieces[i], ref defendingMoves, ref availableEnemyMoves, targetKing);

                if (defendingMoves.Count != 0)
                    return false;
            }

            return true;
        }

        return false;
    }
    //operations

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for(int i=0;i<moves.Count;i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        {
            return false;
        }
        //if another piece is already at that position
        if (chessPieces[x, y] != null)
        {
            ChessPiece otherPiece = chessPieces[x, y];

            if (chessPieces[cp.currentX, cp.currentY].team == otherPiece.team)
            {
                return false;
            }

            if (otherPiece.team == 0)
            {
                if (otherPiece.type == ChessPieceType.King)
                {
                    CheckMate(1);
                }
                deadWhites.Add(otherPiece);
                otherPiece.SetScale(otherPiece.transform.localScale * deadSize);
                otherPiece.SetPosition(new Vector3(8 * tileSize, ((chessPieces[x, y].type == ChessPieceType.Pawn) ? yOffset * 10 : 5 * yOffset), -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deadSpacing) * deadWhites.Count);
            }
            else
            {
                if (otherPiece.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }
                deadBlacks.Add(otherPiece);
                otherPiece.SetScale(otherPiece.transform.localScale * deadSize);
                otherPiece.SetPosition(new Vector3(-1 * tileSize, 5 * yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deadSpacing) * deadBlacks.Count);
            }
        }

        Vector2Int prevPosition = new Vector2Int(cp.currentX, cp.currentY);

        chessPieces[x, y] = cp;
        chessPieces[prevPosition.x, prevPosition.y] = null;
        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        moveList.Add(new Vector2Int[] { prevPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();
        if (CheckCheckmate())
            CheckMate(cp.team);

        return true;
    }
    private  Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for(int x=0;x<TILECOUNT_X;x++)
        {
            for(int y=0;y<TILECOUNT_Y;y++)
            {
                if(tiles[x,y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one;
    }
}
