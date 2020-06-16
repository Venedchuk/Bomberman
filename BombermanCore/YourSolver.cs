/*-
 * #%L
 * Codenjoy - it's a dojo-like platform from developers to developers.
 * %%
 * Copyright (C) 2018 Codenjoy
 * %%
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public
 * License along with this program.  If not, see
 * <http://www.gnu.org/licenses/gpl-3.0.html>.
 * #L%
 */
using Bomberman.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo
{
    /// <summary>
    /// This is BombermanAI client demo.
    /// </summary>
    internal class YourSolver : AbstractSolver
    {
        public YourSolver(string server)
            : base(server)
        {
        }
        int depth = 0;
        Point PlayerPoint;
        List<Point> FutureBlastsPoint;
        List<Point> LastChopperPoint;
        List<Point> PredictChopperPoint;
        List<Point> DangerPoints;
        List<Point> Barriers;
        Board Board;


        /// <summary>
        /// Calls each move to make decision what to do (next move)
        /// </summary>
        protected override string Get(Board board)
        {
            var action = string.Empty;
            if (!board.isMyBombermanDead)
            {
                Board = board;
                Barriers = board.GetBarrier();
                try
                {
                    PlayerPoint = board.GetBomberman();
                }
                catch (Exception)
                {

                    return string.Empty;
                }
                
                FutureBlastsPoint = board.GetFutureBlasts();
                PredictChopperPoint = predictChopper(board.Get(Element.MEAT_CHOPPER));
                DangerPoints = FutureBlastsPoint;
                DangerPoints.AddRange(PredictChopperPoint);
                if (Board.IsAt(PlayerPoint, Element.BOMB_BOMBERMAN))
                {
                    action = findNearElements(Element.Space).ToString();
                }
                else
                {
                    var dir = findNearElements(Element.OTHER_BOMBERMAN);
                    if (dir != null)
                    {
                        action = dir.ToString();

                    }
                    else
                    {
                        dir = findNearElements(Element.DESTROYABLE_WALL);
                        action = dir.ToString();
                    }
                }
            }
            return action;
        }

        private Direction findNearElements(Element element)
        {
            if (Board.IsNear(PlayerPoint, element)&& (element == Element.OTHER_BOMBERMAN))
            {
                return Direction.Act;
            }
            List<WayResolver> wayResolvers = new List<WayResolver>() {new WayResolver(PlayerPoint,Direction.Stop) };
            var waysToTarget = lookAround(element, wayResolvers);

            return waysToTarget;
        }

        private Direction lookAround(Element searchingEl, List<WayResolver> wayResolvers)
        {
            List<Direction> dirlist = new List<Direction>();
            depth = 0;
            for (int i = 0; i < wayResolvers.Count(); i++)
            {
                var nextPoint = wayResolvers[i];
                Checkside(nextPoint.Point.ShiftRight(), Direction.Right, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftLeft(), Direction.Left, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftTop(), Direction.Up, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftBottom(), Direction.Down, searchingEl, wayResolvers);
                if (wayResolvers.Any(way => way.isDestination))
                {

                    Direction firstDir = getReverseWay(wayResolvers, searchingEl, dirlist);
                    return firstDir;
                }
                if (depth > 1000)
                {
                    return Direction.Stop;
                }
            }
            return findNearElements(Element.DESTROYABLE_WALL);

        }
      
        private void Checkside(Point checkedPoint, Direction dir, Element searchingEl, List<WayResolver> wayResolvers)
        {
            if (!checkedPoint.IsOutOf(Board.Size))
            {
                if (!Barriers.Contains(checkedPoint) && !wayResolvers.Any(x => x.Point == checkedPoint))
                {
                    WayResolver way = new WayResolver(checkedPoint, dir);
                    {
                        if (Board.IsNear(checkedPoint, searchingEl))
                        {
                            way.isDestination = true;
                        }
                        if (!DangerPoints.Contains(checkedPoint))
                        {
                            way.isSafe = true;
                        }
                        if (!wayResolvers.Any(x => x.Point == way.Point))
                        {
                            wayResolvers.Add(way);
                        }
                        
                    }
                }
                depth++;
            }
        }

        private Direction getReverseWay(List<WayResolver> wayResolvers, Element searchingEl, List<Direction> dirlist, WayResolver curentItertation = null)
        {
            var previousWay = curentItertation;
            WayResolver lastspot = curentItertation;
            if (curentItertation == null)
            {
                try
                {
                    curentItertation = wayResolvers.First(way => Board.IsNear(way.Point, searchingEl) && way.isSafe);
                }
                catch (Exception)
                {

                    curentItertation = wayResolvers.First(way => Board.IsNear(way.Point, searchingEl));
                }
                
                lastspot = curentItertation;
            }
            switch (curentItertation.Direction)
            {
                case Direction.Left:
                    curentItertation = wayResolvers.Single(way => way.Point == curentItertation.Point.ShiftRight());
                    break;
                case Direction.Right:
                    curentItertation = wayResolvers.Single(way => way.Point == curentItertation.Point.ShiftLeft());
                    break;
                case Direction.Up:
                    curentItertation = wayResolvers.Single(way => way.Point == curentItertation.Point.ShiftBottom());
                    break;
                case Direction.Down:
                    curentItertation = wayResolvers.Single(way => way.Point == curentItertation.Point.ShiftTop());
                    break;
                default:
                    break;
                    
            }
            if (curentItertation.Point != wayResolvers.First().Point)
            {
                curentItertation.Direction = getReverseWay(wayResolvers, searchingEl, dirlist, curentItertation);
            }
            if (curentItertation.Point == wayResolvers.First().Point)
            {
                if (previousWay == null)
                {

                    dirlist.Add(lastspot.Direction);
                    return lastspot.Direction;
                }
                else
                {
                    dirlist.Add(previousWay.Direction);
                    return previousWay.Direction;
                }
            }
            dirlist.Add(curentItertation.Direction);
            return curentItertation.Direction;
        }


        private List<Point> predictChopper(List<Point> currentStates)
        {
            //TO DO predict with previous state

            var dangerArea = new List<Point>();
            foreach (var item in currentStates)
            {
                dangerArea.Add(item);
                dangerArea.Add(item.ShiftBottom());
                dangerArea.Add(item.ShiftLeft());
                dangerArea.Add(item.ShiftRight());
                dangerArea.Add(item.ShiftTop());
            }
            return dangerArea;


            //foreach (var beforePointChopper in previousStates)
            //{
            //    foreach (var currPoint in currentStates)
            //    {
            //        if(beforePointChopper == currPoint)

            //    }
            //}


        }

        private Direction ReverseMove(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    dir = Direction.Right;
                    break;
                case Direction.Right:
                    dir = Direction.Left;
                    break;
                case Direction.Up:
                    dir = Direction.Down;
                    break;
                case Direction.Down:
                    dir = Direction.Up;
                    break;
                case Direction.Act:
                    break;
                case Direction.Stop:
                    break;
            }
            return dir;
        }

    }

    public class WayResolver
    {
        public WayResolver(Point point, Direction dir)
        {
            Point = point;
            Direction = dir;
        }
        public Point Point;
        public Direction Direction;
        public bool isDestination;
        public bool isSafe;

    }


}
