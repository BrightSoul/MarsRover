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

        public event EventHandler<MovedEventArgs>? Moved;
        public Point Location => location;
        public Orientation Orientation => orientation;

        public static Rover CreateAndSendTo(Planet planet, Point landingLocation, Orientation landingOrientation)
        {
            if (planet.HasObstacleAt(landingLocation))
            {
                throw new ObstacleEncounteredException(planet, landingLocation);
            }

            return new Rover(planet, landingLocation, landingOrientation);
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
            Action actuate = command switch
            {
                'f' => MoveForward,
                'b' => MoveBackward,
                'l' => TurnLeft,
                'r' => TurnRight,
                _ => throw new NotSupportedException($"Command '{command}' not supported")
            };

            actuate();
        }

        #region Forward/Backward operations
        private void MoveForward()
        {
            Move(distanceInUnits: 1);
        }

        private void MoveBackward()
        {
            Move(distanceInUnits: -1);
        }

        private void Move(int distanceInUnits)
        {
            if (Math.Abs(distanceInUnits) != 1)
            {
                throw new ArgumentException("This model can only move by one planetary unit at a time");
            }

            Point destination = CalculateDestination(distanceInUnits);
            EnsureNoObstacleIsDetectedAt(destination);

            // TODO: Invoke planet.Occupy(rover, destination) here, in case we wanted to prevent
            // multiple rovers from overlapping. Sending multiple rovers to the same planet
            // was not a use case described in the specification though, so it was not implemented.

            location = destination;
            RaiseMoved();
        }

        private Point CalculateDestination(int distanceInUnits)
        {
            return orientation switch
            {
                Orientation.North => OffsetCurrentLocationBy(0, distanceInUnits * -1),
                Orientation.East => OffsetCurrentLocationBy(distanceInUnits, 0),
                Orientation.South => OffsetCurrentLocationBy(0, distanceInUnits),
                Orientation.West => OffsetCurrentLocationBy(distanceInUnits * -1, 0),
                _ => throw new InvalidOperationException($"Orientation {orientation} not supported")
            };
        }

        private Point OffsetCurrentLocationBy(int offsetX, int offsetY)
        {
            return new Point
            (
                x: WrapValue(location.X + offsetX, planet.Size.Width),
                y: WrapValue(location.Y + offsetY, planet.Size.Height)
            );
        }

        private void EnsureNoObstacleIsDetectedAt(Point destination)
        {
            if (planet.HasObstacleAt(destination))
            {
                throw new ObstacleEncounteredException(planet, destination);
            }
        }
        #endregion

        # region Turn operations
        private void TurnLeft()
        {
            Turn(angleInDegrees: -90);
        }

        private void TurnRight()
        {
            Turn(angleInDegrees: 90);
        }

        private void Turn(int angleInDegrees)
        {
            if (Math.Abs(angleInDegrees) != 90)
            {
                throw new ArgumentException("This model can only turn by one 90 degree increment at a time");
            }

            int finalOrientationInDegrees = WrapValue((int)orientation + angleInDegrees, 360);
            orientation = (Orientation)finalOrientationInDegrees;
            RaiseMoved();
        }
        #endregion

        private void RaiseMoved()
        {
            Moved?.Invoke(this, new MovedEventArgs(location, orientation));
        }

        private static int WrapValue(int value, int topExclusiveBound)
        {
            const int lowerInclusiveBound = 0;
            while (value < lowerInclusiveBound)
            {
                value += topExclusiveBound;
            }

            return value % topExclusiveBound;
        }
    }
}