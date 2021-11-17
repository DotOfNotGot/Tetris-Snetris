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
        static Dictionary<Piece, bool[,]> PieceShapes = new Dictionary<Piece, bool[,]>();
        static void LoadPiecesData()
        {
            foreach (Piece piece in Enum.GetValues(typeof(Piece)))
            {
                string pieceName = Enum.GetName(typeof(Piece), piece);
                PieceShapes[piece] = LoadPieceData($"{pieceName}Piece.txt");
            }
        }

        static bool[,] LoadPieceData(string path)
        {
            bool[,] pieceShape = new bool[4, 4];
            string[] lines = File.ReadAllLines(path);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    pieceShape[x, y] = lines[y][x] != ' ';
                }
            }
            return pieceShape;
        }
        static void DrawPiece(int originX, int originY, Piece piece)
        {
            DrawPiece(originX, originY, piece, "█");
        }

        static void ErasePiece(int originX, int originY, Piece piece)
        {
            DrawPiece(originX, originY, piece, " ");
        }

        static void DrawPiece(int originX, int originY, Piece piece, string symbol)
        {
            bool[,] pieceShape = PieceShapes[piece];
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
        static void DrawPlayArea(int width, int height)
        {

            for (int y = 0; y < height+1; y++)
            {
                for (int x = 0; x < width+2; x++)
                {
                    string symbol = "  ";
                    if ((x == 0 || x == width + 1) && y < height - 1)
                    {
                        symbol = "║";
                    }
                    else if (y == height - 1 && x == 0)
                    {
                        symbol = "╚";
                    }
                    else if (y == height - 1 && x != 0 && x < width+1)
                    {
                        symbol = "══";
                    }
                    else if (y == height - 1 && x == width+1)
                    {
                        symbol = "╝";
                    }
                    Console.Write(symbol);
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            var random = new Random();
            int width = 10;
            int height = 20;
            var playArea = new bool[width, height];
            //Draw the play area.
            DrawPlayArea(width, height);

            //Load all pieces.
            LoadPiecesData();

            //Chooses a random piece and draws it to screen.
            Piece piece = (Piece)random.Next(7);
            int pieceX = 0;
            int pieceY = 0;
            DrawPiece(pieceX, pieceY, piece);

            //Main game loop
            while (true)
            {
                
                //Proceed to next tick.
                Thread.Sleep(250);

                //Drop piece down.
                pieceY++;

                //Erase piece
                ErasePiece(pieceX, pieceY - 1, piece);

                //Redraw the piece in new position.
                DrawPiece(pieceX, pieceY, piece);
            }
            
            Console.SetCursorPosition(0, 22);
        }
    }
}
