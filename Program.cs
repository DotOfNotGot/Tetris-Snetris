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
                    if (pieceShape[x, y])
                    {
                        Console.SetCursorPosition((originX + x) * 2 + 1, originY + y);
                        Console.Write(symbol + symbol);
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

        static int CheckLines(bool[,] playArea, int points)
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
            ClearLines(playArea, points, linesCleared, width, height);
            return points;
        }
        static int ClearLines(bool[,] playArea, int points, Dictionary<int, bool> linesCleared, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                if (linesCleared[y])
                {
                    for (int x = 0; x < width; x++)
                    {
                        //PushLinesDown(playArea, width, height, y);
                        Console.SetCursorPosition(x * 2 + 1, y);
                        if (playArea[x,y])
                        {
                            Console.Write("██");
                        }
                        else
                        {
                            Console.Write("  ");
                        }
                        PushLinesDown(playArea, width, height, y);
                    }
                }
            }
            return points += 100000000;
        }
        static void PushLinesDown(bool[,] playArea, int width, int height, int lineY)
        {
            int storedY;
            for (int y = lineY; y < height; y++)
            {
                storedY = lineY - 1;
                for (int x = 0; x < width; x++)
                {
                    playArea[x, y] = playArea[x, storedY];
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
            int level = 2500000;
            var playArea = new bool[width, height];
            int points = 0;
            int pieceRotation;
            int pieceX;
            int pieceY;
            Piece piece;
            //Load all pieces into the PieceShapes dictionary of bool arrays. Uses enum Piece as keys.
            LoadPiecesData();

            void SpawnPiece()
            {
                piece = (Piece)random.Next(7);
                pieceX = 3;
                pieceY = 0;
                pieceRotation = 0;
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

            //Chooses a random piece and draws it to screen.
            SpawnPiece();

            long tickTimer = DateTime.Now.Ticks;
            //Main game loop
            while (true)
            {
                int fastDrop = 0;

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
                            fastDrop = 5000000;
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
                if (tickTimerNew - tickTimer >= level - fastDrop)
                {
                    tickTimer = tickTimerNew;
                    if (!MovePieceIfPossible(0, 1))
                    {
                        //Chooses a random piece and draws it to screen.
                        points += 100;
                        CheckLines(playArea, points);
                        SpawnPiece();
                        Console.SetCursorPosition(width + width + 2, 0);
                        Console.Write($"Points: {points}");
                    }
                }
                //DrawPlayAreaDebug(playArea);
                Thread.Sleep(1);
            }
        }
    }
}
