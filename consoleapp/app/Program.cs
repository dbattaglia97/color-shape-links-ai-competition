﻿using System;
using System.Threading;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using CommandLine;

namespace ColorShapeLinks.ConsoleApp
{
    class Program
    {
        private Options options;

        static void Main(string[] args)
        {
            Program p;
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.ListPlayers)
                    {
                        foreach (string thinkerName in AIManager.Instance.AIs)
                        {
                            Console.WriteLine(thinkerName);
                        }
                    }
                    else
                    {
                        p = new Program(o);
                        p.Run();
                    }

                });
        }

        private Program(Options options)
        {
            this.options = options;
        }

        private void Run()
        {
            IThinker thinker1 = AIManager.Instance.NewThinker(
                options.Player1, options, options.Player1Params);
            IThinker thinker2 = AIManager.Instance.NewThinker(
                options.Player2, options, options.Player2Params);

            Board board = new Board(options.Rows, options.Cols,
                options.WinSequence, options.RoundPiecesPerPlayer,
                options.SquarePiecesPerPlayer);

            Winner winner = Winner.None;

            Pos[] solution = new Pos[options.WinSequence];

            CancellationTokenSource ts = new CancellationTokenSource();

            while (true)
            {
                int row;
                FutureMove move;

                // Update board view
                Render(board);

                Console.WriteLine(
                    $"Player 1 turn ({PColor.White}, {thinker1})");

                // Determine move for current
                move = thinker1.Think(board, ts.Token);

                // Perform move in game board, get column where move was
                // performed
                row = board.DoMove(move.shape, move.column);

                // If the column had space for the move...
                if (row >= 0)
                {
                    Console.WriteLine(
                        $"Player 1 placed a {move.shape} piece at column {move.column}");

                    // Get possible winner and solution
                    winner = board.CheckWinner(solution);

                    if (winner != Winner.None) break;
                }
                else // If we get here, column didn't have space for the move
                {
                    throw new InvalidOperationException("Invalid move");
                }

                // Update board view
                Render(board);

                Console.WriteLine(
                    $"Player 1 turn ({PColor.White}, {thinker1})");

                // Determine move for current
                move = thinker2.Think(board, ts.Token);

                // Perform move in game board, get column where move was
                // performed
                row = board.DoMove(move.shape, move.column);

                // If the column had space for the move...
                if (row >= 0)
                {
                    Console.WriteLine(
                        $"Player 2 placed a {move.shape} piece at column {move.column}");

                    // Get possible winner and solution
                    winner = board.CheckWinner(solution);

                    if (winner != Winner.None) break;
                }
                else // If we get here, column didn't have space for the move
                {
                    throw new InvalidOperationException("Invalid move");
                }
            }

            if (winner == Winner.Draw)
            {
                Console.WriteLine("Game ended in a draw");
            }
            else if (winner == Winner.White)
            {
                Console.WriteLine($"Winner is {thinker1} ({winner})");
            }
            else if (winner == Winner.Red)
            {
                Console.WriteLine($"Winner is {thinker2} ({winner})");
            }
            else
            {
                throw new InvalidOperationException("Invalid winner!");
            }
        }

        private void Render(Board board)
        {
            for (int r = board.rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < board.cols; c++)
                {
                    char pc = '.';
                    Piece? p = board[r, c];
                    if (p.HasValue)
                    {
                        if (p.Value.Is(PColor.White, PShape.Round))
                            pc = 'w';
                        else if (p.Value.Is(PColor.White, PShape.Square))
                            pc = 'W';
                        else if (p.Value.Is(PColor.Red, PShape.Round))
                            pc = 'r';
                        else if (p.Value.Is(PColor.Red, PShape.Square))
                            pc = 'R';
                        else
                            Console.Error.WriteLine($"Invalid piece {p.Value}");
                    }
                    Console.Write(pc);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}