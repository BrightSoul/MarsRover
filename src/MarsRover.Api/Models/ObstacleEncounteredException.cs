using System;
using System.Drawing;

namespace MarsRover.Api.Models
{
    public class ObstacleEncounteredException : InvalidOperationException
    {
        public ObstacleEncounteredException(Planet planet, Point obstacleLocation)
            : base($"Location {obstacleLocation} is obstructed by an obstacle")
        {
            Planet = planet;
            ObstacleLocation = obstacleLocation;
        }

        public Planet Planet { get; }
        public Point ObstacleLocation { get; }
    }
}