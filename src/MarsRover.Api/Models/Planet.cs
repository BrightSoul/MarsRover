using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MarsRover.Api.Models
{
    public class Planet 
    {
        private readonly HashSet<Point> obstaclesLocations;
        private readonly Rectangle surface;
        private Planet(Size size, IEnumerable<Point> obstaclesLocations)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentException("Size must have a positive width and height values", nameof(size));
            }

            this.obstaclesLocations = obstaclesLocations.ToHashSet();
            this.surface = new Rectangle(Point.Empty, size);
        }

        public static Planet CreateEmpty(Size size)
        {
            return new Planet(size, Enumerable.Empty<Point>());
        }

        public static Planet CreateWithRandomlyGeneratedObstacles(Size size, double obstaclesToEmptySpaceRatio = 0.2)
        {
            // TODO: Extract the obstacle generation logic to an IObstacleGenerator to improve testability
            // This would also allow multiple different generation strategies, if it was requested by the specification
            IEnumerable<Point> obstaclesLocations = GenerateRandomObstacles(size, obstaclesToEmptySpaceRatio);
            return new Planet(size, obstaclesLocations);
        }

        internal static Planet CreateWithGivenObstacles(Size size, IEnumerable<Point> obstacleLocations)
        {
            return new Planet(size, obstacleLocations);
        }

        public Size Size => surface.Size;

        public bool HasObstacleAt(Point location)
        {
            if (!IsLocationWithinSurface(location, surface))
            {
                throw new ArgumentException($"Location {location} must be within the planet surface {surface}", nameof(location));
            }
            
            return obstaclesLocations.Contains(location);
        }

        private static bool IsLocationWithinSurface(Point location, Rectangle surface)
        {
            Rectangle area = new Rectangle(location, new Size(1, 1));
            return area.IntersectsWith(surface);
        }

        private static IEnumerable<Point> GenerateRandomObstacles(Size size, double obstaclesToEmptySpaceRatio)
        {
            if (obstaclesToEmptySpaceRatio < 0.0 || obstaclesToEmptySpaceRatio > 1.0)
            {
                throw new ArgumentException("Obstacles to empty space ratio must be a value between 0.0 and 1.0", nameof(obstaclesToEmptySpaceRatio));
            }

            Random random = new();
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    bool shouldGenerateObstacle = random.NextDouble() <= obstaclesToEmptySpaceRatio;
                    if (shouldGenerateObstacle)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }
    }
}