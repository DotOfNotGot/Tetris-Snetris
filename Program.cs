using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace Tetris
{

    enum Piece
    {
        Line,
        L,
        ReverseL,
        S,
        Square,
        T,
        Z
    }

    class Program
    {
        static Dictionary<Piece, Dictionary<int, bool[,]>> PieceShapes = new Dictionary<Piece, Dictionary<int, bool[,]>>();
        static Dictionary<Piece, int> PieceSpawnOffsets = new Dictionary<Piece, int>{
            {Piece.Line, 0 },{Piece.L, 1 },{Piece.ReverseL, 1 },{Piece.S, 0 },{Piece.Square, 1 },{Piece.T, 0},{Piece.Z, 2 }
        };
        static int[] LevelTickFactors = new int[30] {
            48,43,38,33,28,
            23,18,13,8,6,
            5,5,5,4,4,
            4,3,3,3,2,
            2,2,2,2,2,
            2,2,2,2,1
        };
        static int SoftDropTickDuration = 500000;
        static int GetLevelTickDurations(int level)
        {
            return LevelTickFactors[Math.Min(level, 29)] * 1000000 / 6;
        }

        //Loads all the pieces into the bool array dictionary PieceShapes and all rotations to an inner dictionary to not have to load from file them every time they are used.
        static void LoadPiecesData()
        {
            foreach (Piece piece in Enum.GetValues(typeof(Piece)))
            {
                PieceShapes[piece] = new Dictionary<int, bool[,]>();
                string pieceName = Enum.GetName(typeof(Piece), piece);
                for (int i = 0; i < 4; i++)
                {
                    PieceShapes[piece][i] = LoadPieceData($"{pieceName}Piece.txt", i * 4);
                }
            }
        }

        //Reads the shape of a piece and feeds it into a 4x4 bool array.
        static bool[,] LoadPieceData(string path, int offset)
        {
            bool[,] pieceShape = new bool[4, 4];
            string[] lines = File.ReadAllLines(path);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    pieceShape[x, y] = lines[y + offset][x] != ' ';
                }
            }
            return pieceShape;
        }

        //Uses the other DrawPiece method to draw the piece.
        static void DrawPiece(int originX, int originY, Piece piece, int rotation)
        {
            DrawPiece(originX, originY, piece, "█", rotation);
        }

        //Uses the DrawPiece method to erase the piece.
        static void ErasePiece(int originX, int originY, Piece piece, int rotation)
        {
            DrawPiece(originX, originY, piece, " ", rotation);
        }

        //Method for both drawing and erasing pieces.
        static void DrawPiece(int originX, int originY, Piece piece, string symbol, int rotation)
        {
            bool[,] pieceShape = PieceShapes[piece][rotation];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int targetX = originX + x;
                    int targetY = originY + y;
                    if (pieceShape[x, y])
                    {
                        if (targetY >= 0)
                        {
                            Console.SetCursorPosition(targetX * 2 + 1, targetY);
                            Console.Write(symbol + symbol);
                        }
                    }
                }
            }
        }

        //Sets the play area in the array and draws it out
        static void DrawBorder(int width, int height)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int x = 0; x < width + 2; x++)
                {
                    string symbol = "  ";
                    if ((x == 0 || x == width + 1) && y < height)
                    {
                        symbol = "║";
                    }
                    else if (y == height && x == 0)
                    {
                        symbol = "╚";
                    }
                    else if (y == height && x != 0 && x < width + 1)
                    {
                        symbol = "══";
                    }
                    else if (y == height && x == width + 1)
                    {
                        symbol = "╝";
                    }
                    Console.Write(symbol);
                }
                Console.WriteLine();
            }
        }
        static bool CanAddPiece(int originX, int originY, Piece piece, int rotation, bool[,] playArea)
        {
            int width = playArea.GetLength(0);
            int height = playArea.GetLength(1);
            bool[,] pieceShape = PieceShapes[piece][rotation];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (pieceShape[x, y])
                    {
                        int targetX = originX + x;
                        int targetY = originY + y;
                        if (targetX < width && targetY < height && targetX >= 0)
                        {
                            if (targetY >= 0 && playArea[targetX, targetY])
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        static void AddPiece(int originX, int originY, Piece piece, int rotation, bool[,] playArea)
        {
            PlacePiece(originX, originY, piece, rotation, playArea, true);
        }
        static void RemovePiece(int originX, int originY, Piece piece, int rotation, bool[,] playArea)
        {
            PlacePiece(originX, originY, piece, rotation, playArea, false);
        }
        static void PlacePiece(int originX, int originY, Piece piece, int rotation, bool[,] playArea, bool value)
        {
            int width = playArea.GetLength(0);
            int height = playArea.GetLength(1);
            bool[,] pieceShape = PieceShapes[piece][rotation];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (pieceShape[x, y])
                    {
                        int targetX = originX + x;
                        int targetY = originY + y;
                        if (targetX < width && targetY < height && targetX >= 0 && targetY >= 0)
                        {
                            playArea[targetX, targetY] = value;
                        }
                    }
                }
            }
        }

        static void CheckLines(bool[,] playArea, ref int linesForLevel, ref int points, int level)
        {
            int width = playArea.GetLength(0);
            int height = playArea.GetLength(1);
            int lineCounter = 0;
            Dictionary<int, bool> linesCleared = new Dictionary<int, bool>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (playArea[x, y])
                    {
                        lineCounter++;
                    }
                    if (x == width - 1)
                    {
                        if (lineCounter == width)
                        {
                            linesCleared[y] = true;
                        }
                        else
                        {
                            linesCleared[y] = false;
                        }
                        lineCounter = 0;
                    }
                }
            }
            ClearLines(playArea, linesCleared, width, height, ref linesForLevel, ref points, level);
        }
        static void ClearLines(bool[,] playArea, Dictionary<int, bool> linesCleared, int width, int height, ref int linesForLevel, ref int points, int level)
        {
            points += 100;
            List<int> linesClearedAmount = new List<int>();
            for (int y = 0; y < height; y++)
            {
                if (linesCleared[y])
                {
                    linesClearedAmount.Add(0);
                    for (int x = 0; x < width; x++)
                    {
                        Console.SetCursorPosition(x * 2 + 1, y);
                        playArea[x, y] = false;
                        Console.Write("  ");

                    }
                }
            }
            linesForLevel += linesClearedAmount.Count;
            if (linesClearedAmount.Count == 1)
            {
                points += 40 * (level + 1);
            }
            else if (linesClearedAmount.Count == 2)
            {
                points += 100 * (level + 1);
            }
            else if (linesClearedAmount.Count == 3)
            {
                points += 300 * (level + 1);
            }
            else if (linesClearedAmount.Count == 4)
            {
                points += 1200 * (level + 1);
            }
            
            for (int y = 0; y < height; y++)
            {
                if (linesCleared[y])
                {
                    Thread.Sleep(250);
                    //Pushes all lines down after the lines were cleared.
                    PushLinesDown(playArea, width, y);
                }
            }
        }
        static void PushLinesDown(bool[,] playArea, int width, int lineY)
        {
            for (; lineY > 0; lineY--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (playArea[x, lineY - 1])
                    {
                        playArea[x, lineY] = playArea[x, lineY - 1];
                        playArea[x, lineY - 1] = false;
                        Console.SetCursorPosition(x * 2 + 1, lineY);
                        if (playArea[x, lineY])
                        {
                            Console.Write("██");
                        }
                        else
                        {
                            Console.Write("  ");
                        }
                        Console.SetCursorPosition(x * 2 + 1, lineY - 1);
                        Console.Write("  ");
                    }
                }
            }
        }

        static void DrawPlayAreaDebug(bool[,] playArea)
        {
            int width = playArea.GetLength(0);
            int height = playArea.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.SetCursorPosition((x) * 2 + 1, y);
                    Console.Write(playArea[x, y] ? "██" : "  ");
                }
            }
        }
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            var random = new Random();
            int width = 10;
            int height = 20;
            int tickDuration = GetLevelTickDurations(0);
            var playArea = new bool[width, height];
            int points = 0;
            int linesCleared = 0;
            bool lost = false;
            int level = 0;
            int pieceRotation;
            int pieceX;
            int pieceY;
            Piece piece;
            //Load all pieces into the PieceShapes dictionary of bool arrays. Uses enum Piece as keys.
            LoadPiecesData();

            void SpawnPiece()
            {
                //piece = (Piece)2;
                piece = (Piece)random.Next(7);
                pieceX = 3;
                pieceY = -PieceSpawnOffsets[piece];
                pieceRotation = 0;
                if (!CanAddPiece(pieceX, pieceY, piece, pieceRotation, playArea))
                {
                    lost = true;
                }
                AddPiece(pieceX, pieceY, piece, pieceRotation, playArea);
                DrawPiece(pieceX, pieceY, piece, pieceRotation);
            }
            //Checks if piece can be moved and then moves if it can or remains in same place if it cant.
            bool MovePieceIfPossible(int deltaX, int deltaY)
            {
                RemovePiece(pieceX, pieceY, piece, pieceRotation, playArea);
                if (CanAddPiece(pieceX + deltaX, pieceY + deltaY, piece, pieceRotation, playArea))
                {
                    //Erase piece
                    ErasePiece(pieceX, pieceY, piece, pieceRotation);
                    pieceX += deltaX;
                    pieceY += deltaY;

                    //Redraw the piece in new position.
                    AddPiece(pieceX, pieceY, piece, pieceRotation, playArea);
                    DrawPiece(pieceX, pieceY, piece, pieceRotation);
                    return true;
                }
                else
                {
                    AddPiece(pieceX, pieceY, piece, pieceRotation, playArea);
                    return false;
                }
            }
            //Checks if piece can be rotated and then rotates if it can or remains in same rotation if it cant.
            bool RotatePieceIfPossible(int deltaRotation)
            {
                RemovePiece(pieceX, pieceY, piece, pieceRotation, playArea);
                int targetRotation = (pieceRotation + deltaRotation + 4) % 4;

                if (CanAddPiece(pieceX, pieceY, piece, targetRotation, playArea))
                {
                    //Erase piece
                    ErasePiece(pieceX, pieceY, piece, pieceRotation);
                    pieceRotation = targetRotation;

                    //Redraw the piece in new position.
                    AddPiece(pieceX, pieceY, piece, pieceRotation, playArea);
                    DrawPiece(pieceX, pieceY, piece, pieceRotation);
                    return true;
                }
                else
                {
                    AddPiece(pieceX, pieceY, piece, pieceRotation, playArea);
                    return false;
                }
            }

            //Draw the play area.
            DrawBorder(width, height);
            Console.SetCursorPosition(width + width + 2, 0);
            Console.Write($"Level: {level}");
            Console.SetCursorPosition(width + width + 2, 1);
            Console.Write($"Points: {points}");
            Console.SetCursorPosition(width + width + 2, 2);
            Console.Write($"Lines cleared: {linesCleared}");
            //Chooses a random piece and draws it to screen.
            SpawnPiece();

            long tickTimer = DateTime.Now.Ticks;
            //Main game loop
            while (!lost)
            {
                bool fastDrop = false;
                if (Console.KeyAvailable)
                {
                    var keyPress = Console.ReadKey(true);
                    switch (keyPress.Key)
                    {
                        case ConsoleKey.Escape:
                            Environment.Exit(0);
                            break;
                        case ConsoleKey.LeftArrow:
                            MovePieceIfPossible(-1, 0);
                            break;
                        case ConsoleKey.RightArrow:
                            MovePieceIfPossible(1, 0);
                            break;
                        case ConsoleKey.DownArrow:
                            fastDrop = true;
                            break;
                        case ConsoleKey.Z:
                            RotatePieceIfPossible(-1);
                            break;
                        case ConsoleKey.X:
                            RotatePieceIfPossible(1);
                            break;
                    }
                }

                
                //Checks if a tick has hapenned and if its true drops the piece down by one step.
                long tickTimerNew = DateTime.Now.Ticks;
                if (linesCleared >= (level + 1) * 10)
                {
                    level += 1;
                    tickDuration = GetLevelTickDurations(level);
                    Console.SetCursorPosition(width + width + 2, 0);
                    Console.Write($"Level: {level}");
                }
                if (tickTimerNew - tickTimer >= (fastDrop ? Math.Min(SoftDropTickDuration, tickDuration) : tickDuration) )
                {
                    tickTimer = tickTimerNew;
                    if (!MovePieceIfPossible(0, 1))
                    {
                        CheckLines(playArea, ref linesCleared, ref points, level);
                        //Chooses a random piece and draws it to screen.
                        SpawnPiece();
                        Console.SetCursorPosition(width + width + 2, 1);
                        Console.Write($"Points: {points}");
                        Console.SetCursorPosition(width + width + 2, 2);
                        Console.Write($"Lines cleared: {linesCleared}");
                    }
                }
                //DrawPlayAreaDebug(playArea);
                Thread.Sleep(1);
            }
            Console.SetCursorPosition(0, 22);
            Console.WriteLine("get fucked");
            Thread.Sleep(1000);
        }
    }
}
