using System;
using System.Drawing;

namespace MarsRover.Api.Models
{
    public class Rover
    {
        private readonly Planet planet;
        private Point location;
        private Orientation orientation;
        private Rover(Planet planet, Point location, Orientation orientation)
        {
            this.planet = planet;
            this.location = location;
            this.orientation = orientation;
        }

        public event EventHandler<(Point Location, Orientation Orientation)>? Moved;
        public Point Location => location;
        public Orientation Orientation => orientation;

        public static Rover CreateAndSendTo(Planet planet, Point location, Orientation orientation)
        {
            if (planet.HasObstacleAt(location))
            {
                throw new InvalidOperationException($"Location {location} is not suitable for deployment");
            }

            return new Rover(planet, location, orientation);
        }

        public void ExecuteCommands(params char[] commands)
        {
            foreach (char command in commands)
            {
                ExecuteCommand(command);
            }
        }

        private void ExecuteCommand(char command)
        {
            Action action = command switch
            {
                'f' => GoForward,
                'b' => GoBackward,
                'l' => RotateLeft,
                'r' => RotateRight,
                _ => throw new NotSupportedException($"Command '{command}' not supported")
            };

            action.Invoke();
        }

        private void GoForward()
        {
            Go(distanceInUnits: 1);
        }

        private void GoBackward()
        {
            Go(distanceInUnits: -1);
        }

        private void Go(int distanceInUnits)
        {
            if (distanceInUnits == 0)
            {
                return;
            }

            if (Math.Abs(distanceInUnits) != 1)
            {
                throw new ArgumentException("This model can only move by one planetary unit at a time");
            }

            Point destination = CalculateDestination(distanceInUnits);
            EnsureDestinationIsFreeOfObstacles(destination);

            // TODO: Invoke planet.Occupy(destination) here, in case we wanted to prevent
            // multiple rovers from overlapping. Sending multiple rovers to the same planet
            // was not a use case described in the specification though, so it was not implemented.

            location = destination;
            RaiseMoved();
        }

        private Point CalculateDestination(int distanceInUnits)
        {
            return orientation switch
            {
                Orientation.North => OffsetLocation(0, -1 * distanceInUnits),
                Orientation.East => OffsetLocation(1 * distanceInUnits, 0),
                Orientation.South => OffsetLocation(0, 1 * distanceInUnits),
                Orientation.West => OffsetLocation(-1 * distanceInUnits, 0),
                _ => throw new InvalidOperationException($"Orientation {orientation} not supported")
            };
        }

        private Point OffsetLocation(int offsetX, int offsetY)
        {
            return new Point
            (
                x: (location.X + offsetX + planet.Size.Width) % planet.Size.Width,
                y: (location.Y + offsetY + planet.Size.Height) % planet.Size.Height
            );
        }

        private void EnsureDestinationIsFreeOfObstacles(Point destination)
        {
            if (planet.HasObstacleAt(destination))
            {
                throw new ObstacleEncounteredException(planet, destination);
            }
        }

        private void RotateLeft()
        {
            Rotate(arcInDegrees: 90);
        }

        private void RotateRight()
        {
            Rotate(arcInDegrees: -90);
        }

        private void Rotate(int arcInDegrees)
        {
            if (Math.Abs(arcInDegrees) != 90)
            {
                throw new ArgumentException("This model can only rotate by one 90 degree increment at a time");
            }

            int finalOrientation = (((int)orientation + arcInDegrees) + 360) % 360;
            orientation = (Orientation)finalOrientation;
            RaiseMoved();
        }

        private void RaiseMoved()
        {
            Moved?.Invoke(this, (location, orientation));
        }
    }
}