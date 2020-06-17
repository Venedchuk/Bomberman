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
        int recurcive = 0;

        /// <summary>
        /// Calls each move to make decision what to do (next move)
        /// </summary>
        protected override string Get(Board board)
        {
            var action = string.Empty;
            if (!board.isMyBombermanDead)
            {
                recurcive = 0;
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
                Barriers.AddRange(PredictChopperPoint);
                Barriers.AddRange(Board.GetOtherBombermans());
                Barriers.AddRange(Board.GetMeatChoppers());
                Barriers.AddRange(Board.GetBombs());
                DangerPoints = FutureBlastsPoint;
                DangerPoints.AddRange(PredictChopperPoint);
                if (DangerPoints.Contains(PlayerPoint))
                {
                    action = findNearElements(Element.Space).ToString();
                    
                }
                else
                {
                    var dir = findNearElements(Element.OTHER_BOMBERMAN);
                    Point nextPoint = new Point();
                    switch (dir)
                    {
                        case Direction.Left:
                            nextPoint = PlayerPoint.ShiftLeft();
                            break;
                        case Direction.Right:
                            nextPoint = PlayerPoint.ShiftRight();
                            break;
                        case Direction.Up:
                            nextPoint = PlayerPoint.ShiftTop();
                            break;
                        case Direction.Down:
                            nextPoint = PlayerPoint.ShiftBottom();
                            break;
                        case Direction.Act:
                            break;
                        case Direction.Stop:
                            break;
                        default:
                            break;
                    }
                    if(FutureBlastsPoint.Contains(nextPoint))
                    {
                        dir = Direction.Stop;
                    }
                    if (dir != null)
                    {
                        if (trueWay.Count() > 5)
                        {
                            action = Direction.Act.ToString() + dir.ToString();
                        }
                        else
                        {
                            action = dir.ToString();
                        }
                    }
                }
            }
            else
            {
                FutureBlastsPoint = new List<Point>();
                LastChopperPoint = new List<Point>();
                PredictChopperPoint = new List<Point>();
                DangerPoints = new List<Point>();
                Barriers = new List<Point>();
            }
            return action;
        }

        private Direction findNearElements(Element element)
        {
            if (Board.IsNear(PlayerPoint, element) && ((element == Element.OTHER_BOMBERMAN)||(element == Element.DESTROYABLE_WALL)))
            {
                return Direction.Act;
            }
            List<WayResolver> wayResolvers = new List<WayResolver>() { new WayResolver(PlayerPoint, Direction.Stop) };
            Direction waysToTarget = Direction.Act;
            try
            {
                waysToTarget = lookAround(element, wayResolvers);

            }
            catch (Exception ex)
            {
                waysToTarget = Direction.Act;
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            return waysToTarget;


        }

        private Direction lookAround(Element searchingEl, List<WayResolver> wayResolvers)
        {
            List<Direction> dirlist = new List<Direction>();
            trueWay = new List<Direction>();
            depth = 0;
            int safe = 0;
            for (int i = 0; i < wayResolvers.Count(); i++)
            {
                var nextPoint = wayResolvers[i];
                Checkside(nextPoint.Point.ShiftRight(), Direction.Right, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftLeft(), Direction.Left, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftTop(), Direction.Up, searchingEl, wayResolvers);
                Checkside(nextPoint.Point.ShiftBottom(), Direction.Down, searchingEl, wayResolvers);
                if(wayResolvers.Any(way => way.isSafe && way.isDestination))
                {
                    safe++;
                }
                if (wayResolvers.Any(way => way.isDestination && way.isSafe && !DangerPoints.Contains(way.Point)&& safe >5))
                {

                    Direction firstDir = getReverseWay(wayResolvers, searchingEl, dirlist);
                    trueWay.AddRange(dirlist);
                    return firstDir;
                }
                if (depth > 1100)
                {
                    {
                        if (DangerPoints.Contains(PlayerPoint))
                        {
                            recurcive++;
                            if (recurcive > 2)
                                return Direction.Act;
                            return findNearElements(Element.Space);
                        }
                        else
                        {
                            recurcive++;
                            if (recurcive > 2)
                                return Direction.Act;
                            return findNearElements(Element.DESTROYABLE_WALL);
                        }
                        //return findNearElements(Element.DESTROYABLE_WALL);
                    }
                }
            }
            //here run to safe
            recurcive++;
            if (recurcive > 2)
                return Direction.Act;
            return findNearElements(Element.Space);

        }

        List<Direction> trueWay = new List<Direction>();
        private void Checkside(Point checkedPoint, Direction dir, Element searchingEl, List<WayResolver> wayResolvers)
        {
            if (!checkedPoint.IsOutOf(Board.Size))
            {
                if (!Barriers.Contains(checkedPoint) && !wayResolvers.Any(x => x.Point == checkedPoint))
                {
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
                    curentItertation = new WayResolver(PlayerPoint,Direction.Stop);
                    //curentItertation = wayResolvers.First(way => Board.IsNear(way.Point, searchingEl));
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
