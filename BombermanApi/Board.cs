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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bomberman.Api
{
    public class Board
    {

        private String BoardString { get; }
        private LengthToXY LengthXY;

        public Board(String boardString)
        {
            BoardString = boardString.Replace("\n", "");
            LengthXY = new LengthToXY(Size);
        }

        /// <summary>
        /// GameBoard size (actual board size is Size x Size cells)
        /// </summary>
        public int Size
        {
            get
            {
                return (int)Math.Sqrt(BoardString.Length);
            }
        }

        public Point GetBomberman()
        {
            return Get(Element.BOMBERMAN)
                    .Concat(Get(Element.BOMB_BOMBERMAN))
                    .Concat(Get(Element.DEAD_BOMBERMAN))
                    .Single();
        }

        public List<Point> GetOtherBombermans()
        {
            return Get(Element.OTHER_BOMBERMAN)
                .Concat(Get(Element.OTHER_BOMB_BOMBERMAN))
                .Concat(Get(Element.OTHER_DEAD_BOMBERMAN))
                .ToList();
        }

        public bool isMyBombermanDead
        {
            get
            {
                return BoardString.Contains((char)Element.DEAD_BOMBERMAN);
            }
        }

        public Element GetAt(Point point)
        {
            if (point.IsOutOf(Size))
            {
                return Element.WALL;
            }
            return (Element)BoardString[LengthXY.GetLength(point.X, point.Y)];
        }

        public bool IsAt(Point point, Element element)
        {
            if (point.IsOutOf(Size))
            {
                return false;
            }

            return GetAt(point) == element;
        }

        public string BoardAsString()
        {
            string result = "";
            for (int i = 0; i < Size; i++)
            {
                result += BoardString.Substring(i * Size, Size);
                result += "\n";
            }
            return result;
        }

        /// <summary>
        /// gets board view as string
        /// </summary>
        public string ToString()
        {
            return string.Format("{0}\n" +
                     "Bomberman at: {1}\n" +
                     "Other bombermans at: {2}\n" +
                     "Meat choppers at: {3}\n" +
                     "Destroy walls at: {4}\n" +
                     "Bombs at: {5}\n" +
                     "Blasts: {6}\n" +
                     "Expected blasts at: {7}\n" +
                     "Perks at: {8}",
                     BoardAsString(),
                     GetBomberman(),
                     ListToString(GetOtherBombermans()),
                     ListToString(GetMeatChoppers()),
                     ListToString(GetDestroyableWalls()),
                     ListToString(GetBombs()),
                     ListToString(GetBlasts()),
                     ListToString(GetFutureBlasts()),
                     ListToString(GetPerks()));
        }

        private string ListToString(List<Point> list)
        {
            return string.Join(",", list.ToArray());
        }

        public List<Point> GetBarrier()
        {
            return
                GetWalls()
                .Concat(GetOtherBombermans())
                .Concat(GetBombs())
                .Concat(GetMeatChoppers())
                .Concat(GetDestroyableWalls())
                .Distinct()
                .ToList();
        }

        public List<Point> GetMeatChoppers()
        {
            return Get(Element.MEAT_CHOPPER);
        }

        public List<Point> Get(Element element)
        {
            List<Point> result = new List<Point>();

            for (int i = 0; i < Size * Size; i++)
            {
                Point pt = LengthXY.GetXY(i);

                if (IsAt(pt, element))
                {
                    result.Add(pt);
                }
            }

            return result;
        }

        public List<Point> GetWalls()
        {
            return Get(Element.WALL);
        }

        public List<Point> GetDestroyableWalls()
        {
            return Get(Element.DESTROYABLE_WALL);
        }

        public List<Point> GetBombs(Point FutureBombPoint = new Point())
        {
            return Get(Element.BOMB_TIMER_1)
                .Concat(Get(Element.BOMB_TIMER_2))
                .Concat(Get(Element.BOMB_TIMER_3))
                .Concat(Get(Element.BOMB_TIMER_4))
                .Concat(Get(Element.BOMB_TIMER_5))
                .Concat(Get(Element.BOMB_BOMBERMAN))
                .Concat(Get(Element.OTHER_BOMB_BOMBERMAN))
                .Append(FutureBombPoint)
                .ToList();
        }

        public List<Point> GetPerks()
        {
            return Get(Element.BOMB_BLAST_RADIUS_INCREASE)
                .Concat(Get(Element.BOMB_COUNT_INCREASE))
                .Concat(Get(Element.BOMB_IMMUNE))
                .Concat(Get(Element.BOMB_REMOTE_CONTROL))
                .ToList();
        }

        public List<Point> GetBlasts()
        {
            return Get(Element.BOOM);
        }

        public List<Point> GetFutureBlasts(Point predictedBombs = new Point())
        {
            var bombs = GetBombs();
            bombs.Add(predictedBombs);
            var result = new List<Point>();
            foreach (var bomb in bombs)
            {
                result.Add(bomb);
                result.Add(bomb.ShiftLeft());
                result.Add(bomb.ShiftRight());
                result.Add(bomb.ShiftTop());
                result.Add(bomb.ShiftBottom());
                if (!this.IsBarrierAt(bomb.ShiftLeft()))
                {
                    result.Add(bomb.ShiftLeft(2));
                    if (!this.IsBarrierAt(bomb.ShiftLeft(2)))
                    {
                        result.Add(bomb.ShiftLeft(3));
                    }
                }

                if (!this.IsBarrierAt(bomb.ShiftRight()))
                {
                    result.Add(bomb.ShiftRight(2));
                    if (!this.IsBarrierAt(bomb.ShiftRight(2)))
                    {
                        result.Add(bomb.ShiftRight(3));
                    }
                }
                if (!this.IsBarrierAt(bomb.ShiftTop()))
                {
                    result.Add(bomb.ShiftTop(2));
                    if (!this.IsBarrierAt(bomb.ShiftTop(2)))
                    {
                        result.Add(bomb.ShiftTop(3));
                    }
                }
                if (!this.IsBarrierAt(bomb.ShiftBottom()))
                {
                    result.Add(bomb.ShiftBottom(2));
                    if (!this.IsBarrierAt(bomb.ShiftBottom(2)))
                    {
                        result.Add(bomb.ShiftBottom(3));
                    }
                }
            }

            return result.Where(blast => !blast.IsOutOf(Size) && !GetWalls().Contains(blast)).Distinct().ToList();
        }

        public bool IsAnyOfAt(Point point, params Element[] elements)
        {
            return elements.Any(elem => IsAt(point, elem));
        }

        public bool IsNear(Point point, Element element)
        {
            if (point.IsOutOf(Size))
                return false;

            return IsAt(point.ShiftLeft(), element) ||
                   IsAt(point.ShiftRight(), element) ||
                   IsAt(point.ShiftTop(), element) ||
                   IsAt(point.ShiftBottom(), element);
        }
        public bool IsDestroyableNear(Point point)
        {
            List<Element> enemy = new List<Element>()
            {
                Element.DESTROYABLE_WALL,
                Element.OTHER_BOMBERMAN,
                Element.MEAT_CHOPPER
            };
            if (point.IsOutOf(Size))
                return false;
            bool desicion = false;

            if (!this.IsAt(point.ShiftLeft(1), Element.WALL))
            {
                if (enemy.Contains(this.GetAt(point.ShiftLeft(1))))
                {
                    desicion = true;
                }
                if (!this.IsAt(point.ShiftLeft(2), Element.WALL))
                {
                    if (enemy.Contains(this.GetAt(point.ShiftLeft(2))))
                    {
                        desicion = true;
                    }

                    if (!this.IsAt(point.ShiftLeft(3), Element.WALL))
                    {
                        if (enemy.Contains(this.GetAt(point.ShiftLeft(3))))
                        {
                            desicion = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftRight(1), Element.WALL))
            {
                if (enemy.Contains(this.GetAt(point.ShiftRight(1))))
                {
                    desicion = true;
                }
                if (!this.IsAt(point.ShiftRight(2), Element.WALL))
                {
                    if (enemy.Contains(this.GetAt(point.ShiftRight(2))))
                    {
                        desicion = true;
                    }

                    if (!this.IsAt(point.ShiftRight(3), Element.WALL))
                    {
                        if (enemy.Contains(this.GetAt(point.ShiftRight(3))))
                        {
                            desicion = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftTop(1), Element.WALL))
            {
                if (enemy.Contains(this.GetAt(point.ShiftTop(1))))
                {
                    desicion = true;
                }
                if (!this.IsAt(point.ShiftTop(2), Element.WALL))
                {
                    if (enemy.Contains(this.GetAt(point.ShiftTop(2))))
                    {
                        desicion = true;
                    }

                    if (!this.IsAt(point.ShiftTop(3), Element.WALL))
                    {
                        if (enemy.Contains(this.GetAt(point.ShiftTop(3))))
                        {
                            desicion = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftBottom(1), Element.WALL))
            {
                if (enemy.Contains(this.GetAt(point.ShiftBottom(1))))
                {
                    desicion = true;
                }
                if (!this.IsAt(point.ShiftBottom(2), Element.WALL))
                {
                    if (enemy.Contains(this.GetAt(point.ShiftBottom(2))))
                    {
                        desicion = true;
                    }

                    if (!this.IsAt(point.ShiftBottom(3), Element.WALL))
                    {
                        if (enemy.Contains(this.GetAt(point.ShiftBottom(3))))
                        {
                            desicion = true;
                        }
                    }
                }
            }
            var percsPoint = GetPerks();
            bool BonusContain = false;

            //help me

            if (!this.IsAt(point.ShiftLeft(1), Element.WALL))
            {
                if (percsPoint.Contains(point.ShiftLeft(1)))
                {
                    BonusContain = true;
                }
                if (!this.IsAt(point.ShiftLeft(2), Element.WALL))
                {
                    if (percsPoint.Contains((point.ShiftLeft(2))))
                    {
                        BonusContain = true;
                    }

                    if (!this.IsAt(point.ShiftLeft(3), Element.WALL))
                    {
                        if (percsPoint.Contains((point.ShiftLeft(3))))
                        {
                            BonusContain = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftRight(1), Element.WALL))
            {
                if (percsPoint.Contains((point.ShiftRight(1))))
                {
                    BonusContain = true;
                }
                if (!this.IsAt(point.ShiftRight(2), Element.WALL))
                {
                    if (percsPoint.Contains((point.ShiftRight(2))))
                    {
                        BonusContain = true;
                    }

                    if (!this.IsAt(point.ShiftRight(3), Element.WALL))
                    {
                        if (percsPoint.Contains((point.ShiftRight(3))))
                        {
                            BonusContain = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftTop(1), Element.WALL))
            {
                if (percsPoint.Contains((point.ShiftTop(1))))
                {
                    BonusContain = true;
                }
                if (!this.IsAt(point.ShiftTop(2), Element.WALL))
                {
                    if (percsPoint.Contains((point.ShiftTop(2))))
                    {
                        BonusContain = true;
                    }

                    if (!this.IsAt(point.ShiftTop(3), Element.WALL))
                    {
                        if (percsPoint.Contains((point.ShiftTop(3))))
                        {
                            BonusContain = true;
                        }
                    }
                }
            }
            if (!this.IsAt(point.ShiftBottom(1), Element.WALL))
            {
                if (percsPoint.Contains((point.ShiftBottom(1))))
                {
                    BonusContain = true;
                }
                if (!this.IsAt(point.ShiftBottom(2), Element.WALL))
                {
                    if (percsPoint.Contains((point.ShiftBottom(2))))
                    {
                        BonusContain = true;
                    }

                    if (!this.IsAt(point.ShiftBottom(3), Element.WALL))
                    {
                        if (percsPoint.Contains((point.ShiftBottom(3))))
                        {
                            BonusContain = true;
                        }
                    }
                }
            }

            if (!BonusContain)
            {
                return desicion;
            }
            return false;

        }

        public bool IsBarrierAt(Point point)
        {
            return GetBarrier().Contains(point);
        }

        public int CountNear(Point point, Element element)
        {
            if (point.IsOutOf(Size))
                return 0;

            int count = 0;
            if (IsAt(point.ShiftLeft(), element)) count++;
            if (IsAt(point.ShiftRight(), element)) count++;
            if (IsAt(point.ShiftTop(), element)) count++;
            if (IsAt(point.ShiftBottom(), element)) count++;
            return count;
        }
    }
}
