/// @file
/// @brief This file contains the
/// ::ColorShapeLinks.Common.AI.Examples.MinimaxAIThinker class.
///
/// @author Nuno Fachada
/// @date 2020, 2021
/// @copyright [MPLv2](http://mozilla.org/MPL/2.0/)
//using UnityEngine;
using System;
using System.Threading;

namespace ColorShapeLinks.Common.AI.Examples
{
    /// <summary>
    /// Sample AI thinker using a basic Minimax algorithm with a naive
    /// heuristic which previledges center board positions.
    /// </summary>
    /// <remarks>
    /// This is the same implementation used in the @ref minimax tutorial.
    /// </remarks>
    public class MyMiniMax : AbstractThinker
    {
        // Maximum Minimax search depth.
        private int maxDepth;

        /// <summary>
        /// The default maximum search depth.
        /// </summary>
        public const int defaultMaxDepth = 3;

        /// <summary>
        /// Setups up this thinker's maximum search depth.
        /// </summary>
        /// <param name="str">
        /// A string which should be convertible to a positive `int`.
        /// </param>
        /// <remarks>
        /// If <paramref name="str"/> is not convertible to a positive `int`,
        /// the maximum search depth is set to <see cref="defaultMaxDepth"/>.
        /// </remarks>
        /// <seealso cref="ColorShapeLinks.Common.AI.AbstractThinker.Setup"/>
        public override void Setup(string str)
        {
            // Try to get the maximum depth from the parameters
            if (!int.TryParse(str, out maxDepth))
            {
                // If not possible, set it to the default
                maxDepth = defaultMaxDepth;
            }

            // If a non-positive integer was provided, reset it to the default
            if (maxDepth < 1) maxDepth = defaultMaxDepth;
        }

        /// <summary>
        /// Returns the name of this AI thinker which will include the
        /// maximum search depth.
        /// </summary>
        /// <returns>The name of this AI thinker.</returns>
        public override string ToString()
        {
            return base.ToString() + "D" + maxDepth;
        }

        /// @copydoc IThinker.Think
        /// <seealso cref="IThinker.Think"/>
        public override FutureMove Think(Board board, CancellationToken ct)
        {
            // Invoke minimax, starting with zero depth
            (FutureMove move, float score) decision =
                MinimaxAlphaBeta(board, ct, board.Turn, board.Turn, 0,float.NegativeInfinity,float.PositiveInfinity);

            // Return best move
            return decision.move;
        }

        /// <summary>
        /// A basic implementation of the Minimax algorithm.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="player">
        /// Color of the AI controlling this thinker.
        /// </param>
        /// <param name="turn">
        /// Color of the player playing in this turn.
        /// </param>
        /// <param name="depth">Current search depth.</param>
        /// <returns>
        /// A value tuple with:
        /// <list type="bullet">
        /// <item>
        /// <term><c>move</c></term>
        /// <description>
        /// The best move from the perspective of who's playing in this turn.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>score</c></term>
        /// <description>
        /// The heuristic score associated with <c>move</c>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        private (FutureMove move, float score) MinimaxAlphaBeta(
            Board board, CancellationToken ct,
            PColor player, PColor turn, int depth,float alpha,float beta)
        {
            // Move to return and its heuristic value
            (FutureMove move, float score) selectedMove;

            // Current board state
            Winner winner;


            // If a cancellation request was made...
            if (ct.IsCancellationRequested)
            {
                // ...set a "no move" and skip the remaining part of
                // the algorithm
                selectedMove = (FutureMove.NoMove, float.NaN);
            }
            // Otherwise, if it's a final board, return the appropriate
            // evaluation
            else if ((winner = board.CheckWinner()) != Winner.None)
            {
                if (winner.ToPColor() == player)
                {
                    // AI player wins, return highest possible score
                    selectedMove = (FutureMove.NoMove, float.PositiveInfinity);
                }
                else if (winner.ToPColor() == player.Other())
                {
                    // Opponent wins, return lowest possible score
                    selectedMove = (FutureMove.NoMove, float.NegativeInfinity);
                }
                else
                {
                    // A draw, return zero
                    selectedMove = (FutureMove.NoMove, 0f);
                }
            }
            // If we're at maximum depth and don't have a final board, use
            // the heuristic
            else if (depth == maxDepth)
            {
                selectedMove = (FutureMove.NoMove, Heuristic(board, player));
            }
            else // Board not final and depth not at max...
            {
                //...so let's test all possible moves and recursively call
                // Minimax() for each one of them, maximizing or minimizing
                // depending on who's turn it is

                // Initialize the selected move...
                selectedMove = turn == player
                    // ...with negative infinity if it's the AI's turn and
                    // we're maximizing (so anything except defeat will be
                    // better than this)
                    ? (FutureMove.NoMove, float.NegativeInfinity)
                    // ...or with positive infinity if it's the opponent's
                    // turn and we're minimizing (so anything except victory
                    // will be worse than this)
                    : (FutureMove.NoMove, float.PositiveInfinity);

                // Test each column
                for (int i = 0; i < Cols; i++)
                {
                    // Skip full columns
                    bool breakLoops = false;
                    if (board.IsColumnFull(i)) continue;

                    // Test shapes
                    for (int j = 0; j < 2; j++)
                    {
                        // Get current shape
                        PShape shape = (PShape)j;

                        // Use this variable to keep the current board's score
                        float eval;
                

                        // Skip unavailable shapes
                        if (board.PieceCount(turn, shape) == 0) continue;

                        // Test move, call minimax and undo move
                        board.DoMove(shape, i);
                        eval = MinimaxAlphaBeta(
                            board, ct, player, turn.Other(), depth + 1,alpha,beta).score;
                        board.UndoMove();

                        // If we're maximizing, is this the best move so far?
                        if (turn == player){
                          if(eval>= selectedMove.score)selectedMove = (new FutureMove(i, shape), eval);
                          if(eval>=beta){
                            breakLoops = true;
                            break; //Beta cutoff
                           }
                          alpha= Math.Max(alpha,eval);
                        }
                        else if (turn == player.Other()){
                          if(eval <= selectedMove.score)selectedMove=(new FutureMove(i, shape), eval);
                          if(eval<=alpha){breakLoops = true; break; }//Alpha cutoff
                          beta= Math.Min(beta,eval);
                        }
                    }
                    if (breakLoops)
                    {
                        break;
                    }
                }
            }
            // Return movement and its heuristic value
            return selectedMove;
        }

        /// <summary>
        /// Naive heuristic function which previledges center board positions.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="color">
        /// Perspective from which the board will be evaluated.
        /// </param>
        /// <returns>
        /// The heuristic value of the given <paramref name="board"/> from
        /// the perspective of the specified <paramref name="color"/.
        /// </returns>
        private float Heuristic(Board board, PColor color)
        {
            // Current heuristic value
            float h = 0;
            float winningStrike=board.piecesInSequence;

            float hZoneStart =winningStrike-1;
            float vZoneEnd =  winningStrike-1;
            float hZoneEnd = board.cols- winningStrike;
            float vZoneStart = board.rows- winningStrike;


            float HorizontalHeuristic(float x)
            {
                if(x>=hZoneStart && x<=hZoneEnd){
                    return (float)(winningStrike+1);
                }
                else if(x<hZoneStart){
                    return x;
                }
                return board.cols-x;  
            }
            float VerticalHeuristic(float y)
            {
                if(y>=vZoneStart && y<=vZoneEnd){
                    return (float)(winningStrike+1);
                }
                else if(y<vZoneStart){
                    return y;
                }
                else{
                    return board.rows-y;
                }
            }
            float BuildFromAfar(float x,float y){
                float val=0;
                if(x<winningStrike||x>board.cols-winningStrike) return val;

                for (int j=(int)y; j<(int)y+winningStrike-1;j++){
                    Piece? piece = board[(int)x, j];
                    // Is there any piece there?
                    if (piece.HasValue)
                    {
                        if (!(piece.Value.color == color || piece.Value.shape == color.Shape() )){
                            break;
                        }else{
                            val+= j-(int)y;
                        } 
                    }
                }
                return val;                
            }

            float DumpFromAfar(float x,float y){
                float val=0;
                Piece? piece = board[(int)x, (int)y];
                if (piece.Value.color == color && piece.Value.shape != color.Shape() ){
                    val -= - HorizontalHeuristic(x)/3 + (2*VerticalHeuristic(y))/3;
                }
                return val;
            }


            
            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    // Get piece in current board position
                    Piece? piece = board[i, j];
                    // Is there any piece there?
                    if (piece.HasValue)
                    {
                        
                        // If the piece is of our color, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.color == color){
                            h += HorizontalHeuristic(i);
                            h += VerticalHeuristic(j);
                            h += BuildFromAfar(i,j);
                            h += DumpFromAfar(i,j);
                        }
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else{
                            h -= HorizontalHeuristic(i);
                            h -= VerticalHeuristic(j);
                            h -= BuildFromAfar(i,j);
                        }
                        // If the piece is of our shape, increment the
                        // heuristic inversely to the distance from the center
                        if (piece.Value.shape == color.Shape()){
                            h += HorizontalHeuristic(i);
                            h += VerticalHeuristic(j);
                            h += BuildFromAfar(i,j);
                        }
                        // Otherwise decrement the heuristic value using the
                        // same criteria
                        else{
                            h -= HorizontalHeuristic(i);
                            h -= VerticalHeuristic(j);
                            h -= BuildFromAfar(i,j);
                        }
                    }
                }
            }
            // Return the final heuristic score for the given board
            return h;
        }
    }
}