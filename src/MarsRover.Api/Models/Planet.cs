using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MarsRover.Api.Models
{
    public class Planet 
    {
        private readonly HashSet<Point> obstacles;
        private readonly Rectangle surface;
        public Planet(Size size, double obstaclesRatio)
        {
            if (size.Width < 0 || size.Height < 0)
            {
                throw new ArgumentException("Size must have a positive width and height values", nameof(size));
            }

            if (obstaclesRatio < 0.0 || obstaclesRatio > 1.0)
            {
                throw new ArgumentException("Obstacles to empty space ratio must be a value between 0.0 and 1.0", nameof(obstaclesRatio));
            }
             
            // TODO: Extract the obstacle generation logic to an IObstacleGenerator to improve testability
            obstacles = RandomlyGenerateObstacles(size, obstaclesRatio).ToHashSet();
            surface = new Rectangle(Point.Empty, size);
        }

        public Size Size => surface.Size;

        public bool HasObstacleAt(Point coordinate)
        {
            Rectangle location = new Rectangle(coordinate, new Size(1, 1));
            if (!location.IntersectsWith(surface))
            {
                throw new ArgumentException($"Coordinate {coordinate} is out of the planet surface", nameof(location));
            }
            
            return obstacles.Contains(coordinate);
        }

        private static IEnumerable<Point> RandomlyGenerateObstacles(Size size, double obstaclesRatio)
        {
            Random random = new();
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    bool shouldGenerateObstacle = random.NextDouble() <= obstaclesRatio;
                    if (shouldGenerateObstacle)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }
    }
}