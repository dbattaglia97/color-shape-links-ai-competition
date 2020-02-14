/// @file
/// @brief This file contains the ::HumanThinker class.
///
/// @author Nuno Fachada
/// @date 2020
/// @copyright [MPLv2](http://mozilla.org/MPL/2.0/)

using System;
using System.Threading;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;

namespace ColorShapeLinks.ConsoleApp
{
    /// <summary>
    /// A human thinker for testing the console app.
    /// </summary>
    public class HumanThinker : AbstractThinker
    {
        // Currently selected column
        private int selectedCol;

        // Currently selected shape
        private PShape selectedShape;

        /// @copydoc AbstractThinker.Setup
        /// <seealso cref="AbstractThinker.Setup"/>
        public override void Setup(string str)
        {
            selectedCol = Cols / 2;
            selectedShape = PShape.Round;
        }

        /// @copybrief IThinker.Think
        /// <remarks>
        /// This method asks the human to play.
        /// </remarks>
        /// <seealso cref="IThinker.Think"/>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            // By default, no move is performed in case of timeout
            FutureMove move = FutureMove.NoMove;

            // Calculate the time limit
            DateTime timeLimit =
                DateTime.Now + TimeSpan.FromMilliseconds(TimeLimitMillis);

            // No thinking notification has taken place
            DateTime lastNotificationTime = DateTime.MinValue;

            // Set thinking notification interval between frames to 20ms
            TimeSpan notificationInterval = TimeSpan.FromMilliseconds(20);

            // If a certain type of piece is not available, make the other
            // type the selected one
            if (board.PieceCount(board.Turn, PShape.Round) == 0)
                selectedShape = PShape.Square;
            if (board.PieceCount(board.Turn, PShape.Square) == 0)
                selectedShape = PShape.Round;

            // If the current column is full, iterate column selection until
            // one is not
            while (board.IsColumnFull(selectedCol))
            {
                selectedCol++;
                if (selectedCol >= Cols)
                    selectedCol = 0;
            }

            // Wait for human input at most until the cancellation token is
            // activated
            while (true)
            {
                // Was a key pressed?
                if (Console.KeyAvailable)
                {
                    // Retrieve key
                    ConsoleKey key = Console.ReadKey().Key;

                    // Check if it was a column increment key
                    if (key == ConsoleKey.RightArrow
                        || key == ConsoleKey.D
                        || key == ConsoleKey.NumPad6)
                    {
                        // Increment column...
                        do
                        {
                            // ...making sure a full column is not selectable,
                            // and wraping around
                            selectedCol++;
                            if (selectedCol >= Cols)
                                selectedCol = 0;
                        } while (board.IsColumnFull(selectedCol));
                    }
                    // Check if it was a column decrement key
                    else if (key == ConsoleKey.LeftArrow
                        || key == ConsoleKey.A
                        || key == ConsoleKey.NumPad4)
                    {
                        // Decrement column...
                        do
                        {
                            // ...making sure a full column is not selectable,
                            // and wraping around
                            selectedCol--;
                            if (selectedCol < 0)
                                selectedCol = Cols - 1;
                        } while (board.IsColumnFull(selectedCol));
                    }
                    // Check if it was a piece toggle key
                    else if (key == ConsoleKey.T)
                    {
                        // Toggle piece if the other type of piece is available
                        if (selectedShape == PShape.Round
                            && board.PieceCount(board.Turn, PShape.Square) > 0)
                        {
                            selectedShape = PShape.Square;
                        }
                        else if (selectedShape == PShape.Square
                            && board.PieceCount(board.Turn, PShape.Round) > 0)
                        {
                            selectedShape = PShape.Round;
                        }
                    }
                    // Check if it was a piece drop key
                    else if (key == ConsoleKey.Enter)
                    {
                        // Drop piece and get out of the input loop
                        move = new FutureMove(selectedCol, selectedShape);
                        break;
                    }
                }

                // If cancellation token is activated, terminate input loop
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // Is it time for another thinking notification?
                if (DateTime.Now > lastNotificationTime + notificationInterval)
                {
                    // If so, raise notification event with thinking
                    // information
                    OnThinkingInfo(new string[] {
                        $"< > : Column [{selectedCol,8}] selected   ",
                        $" T  : Piece  [{selectedShape,8}] selected  ",
                        $"    : Time to play: {timeLimit - DateTime.Now, 14}"
                    });

                    // Update last notification time
                    lastNotificationTime = DateTime.Now;
                }
            }

            // Return chosen move
            return move;
        }
    }
}