using System;
using System.Drawing;

namespace MarsRover.Api.Models
{
    public class ObstacleEncounteredException : InvalidOperationException
    {
        public ObstacleEncounteredException(Planet planet, Point obstacleLocation)
            : base($"Could not reach destination: an obstacle was encountered at {obstacleLocation}")
        {
            Planet = planet;
            ObstacleLocation = obstacleLocation;
        }

        public Planet Planet { get; }
        public Point ObstacleLocation { get; }
    }
}