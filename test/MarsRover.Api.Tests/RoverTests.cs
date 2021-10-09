using System.Drawing;
using MarsRover.Api.Models;
using NUnit.Framework;

namespace MarsRover.Api.Test
{
    public class RoverTests
    {
        private const char RotateLeftCommand = 'l';
        private const char RotateRightCommand = 'r';
        private const char GoForwardCommand = 'f';
        private const char GoBackwardCommand = 'b';

        [Test]
        [TestCase(GoForwardCommand, Orientation.North, 1, 1, 1, 0)] // In the middle, going north
        [TestCase(GoForwardCommand, Orientation.East, 1, 1, 2, 1)]  // In the middle, going east
        [TestCase(GoForwardCommand, Orientation.South, 1, 1, 1, 2)] // In the middle, going south
        [TestCase(GoForwardCommand, Orientation.West, 1, 1, 0, 1)]  // In the middle, going west
        [TestCase(GoForwardCommand, Orientation.North, 1, 0, 1, 2)] // At the top edge, going north
        [TestCase(GoForwardCommand, Orientation.East, 2, 1, 0, 1)]  // At the right edge, going east
        [TestCase(GoForwardCommand, Orientation.South, 1, 2, 1, 0)] // At the bottom edge, going south
        [TestCase(GoForwardCommand, Orientation.West, 0, 1, 2, 1)]  // At the left edge, going west
        [TestCase(GoBackwardCommand, Orientation.North, 1, 1, 1, 2)] // In the middle, reversing south
        [TestCase(GoBackwardCommand, Orientation.East, 1, 1, 0, 1)]  // In the middle, reversing west
        [TestCase(GoBackwardCommand, Orientation.South, 1, 1, 1, 0)] // In the middle, reversing north
        [TestCase(GoBackwardCommand, Orientation.West, 1, 1, 2, 1)]  // In the middle, reversing east
        [TestCase(GoBackwardCommand, Orientation.North, 1, 2, 1, 0)] // At the bottom edge, reversing south
        [TestCase(GoBackwardCommand, Orientation.East, 0, 1, 2, 1)]  // At the left edge, reversing west
        [TestCase(GoBackwardCommand, Orientation.South, 1, 0, 1, 2)] // At the top edge, reversing south
        [TestCase(GoBackwardCommand, Orientation.West, 2, 1, 0, 1)]  // At the right edge, reversing east
        public void ExecuteCommands_UpdatesLocationAndKeepsOrientation_WhenMovingForwardOrBackwardWithNoObstacles(char command, Orientation deployOrientation, int deployLocationX, int deployLocationY, int expectedFinalLocationX, int expectedFinalLocationY)
        {
            // Arrange
            Point expectedFinalLocation = new(expectedFinalLocationX, expectedFinalLocationY);
            Orientation expectedOrientation = deployOrientation;
            Size size = new(3, 3);
            Planet planet = Planet.CreateEmpty(size);
            Point deployLocation = new(deployLocationX, deployLocationY);
            Rover rover = Rover.CreateAndSendTo(planet, deployLocation, deployOrientation);
            Point? actualNotifiedLocation = null;
            Orientation? actualNotifiedOrientation = null;

            rover.Moved += (sender, args) =>
            {
                actualNotifiedLocation = args.Location;
                actualNotifiedOrientation = args.Orientation;
            };

            // Act
            rover.ExecuteCommands(command);

            // Assert
            Assert.AreEqual(expectedFinalLocation, actualNotifiedLocation);
            Assert.AreEqual(expectedFinalLocation, rover.Location);
            Assert.AreEqual(expectedOrientation, actualNotifiedOrientation);
            Assert.AreEqual(expectedOrientation, rover.Orientation);
        }

        [Test]
        [TestCase(GoForwardCommand, Orientation.West, 0, 0, 1, 0)]  // At the left edge, going west
        [TestCase(GoBackwardCommand, Orientation.West, 1, 0, 0, 0)]  // At the left edge, going west
        public void ExecuteCommands_RaisesException_WhenMovingForwardOrBackwardToAnObstacle(char command, Orientation deployOrientation, int deployLocationX, int deployLocationY, int destinationX, int destinationY)
        {
            // Arrange
            Size size = new(2, 1);
            Point obstacleLocation = new Point(destinationX, destinationY);
            Planet planet = Planet.CreateWithGivenObstacles(size, new [] { obstacleLocation });
            Point deployLocation = new(deployLocationX, deployLocationY);
            Rover rover = Rover.CreateAndSendTo(planet, deployLocation, deployOrientation);

            // Act & Assert
            ObstacleEncounteredException? exc = Assert.Throws<ObstacleEncounteredException>(() => 
            {
                rover.ExecuteCommands(command);
            });

            Assert.AreEqual(exc?.ObstacleLocation, obstacleLocation);
        }

        [Test]
        [TestCase(RotateLeftCommand, Orientation.West, Orientation.South, Orientation.East, Orientation.North)]
        [TestCase(RotateRightCommand, Orientation.East, Orientation.South, Orientation.West, Orientation.North)]
        public void ExecuteCommands_UpdatesOrientationAndKeepsLocation_WhenRotatingLeftOrRight(char command, params Orientation[] expectedOrientations)
        {
            // Arrange
            Size size = new(3, 3);
            Planet planet = Planet.CreateEmpty(size);
            Point deployLocation = new(1, 1);
            Point expectedLocation = deployLocation;
            Orientation deployOrientation = Orientation.North;
            Orientation? actualNotifiedOrientation = null;
            Point? actualNotifiedLocation = null;
            Rover rover = Rover.CreateAndSendTo(planet, deployLocation, deployOrientation);
            rover.Moved += (sender, args) =>
            {
                actualNotifiedLocation = args.Location;
                actualNotifiedOrientation = args.Orientation;
            };

            // Act & Assert
            foreach(Orientation expectedOrientation in expectedOrientations)
            {
                rover.ExecuteCommands(command);
                Assert.AreEqual(expectedOrientation, rover.Orientation);
                Assert.AreEqual(expectedLocation, rover.Location);
                Assert.AreEqual(expectedOrientation, actualNotifiedOrientation);
                Assert.AreEqual(expectedLocation, actualNotifiedLocation);
            }
        }

        [Test]
        public void ExecuteCommands_MovesUpToTheLastPossiblePoint_WhenObstacleIsEncountered()
        {
            // Arrange
            Size size = new (3, 3);
            Point obstacleLocation = new Point(1, 2);
            Planet planet = Planet.CreateWithGivenObstacles(size, new [] { obstacleLocation });
            Point deployLocation = new Point(0, 0);
            Orientation deployOrientation = Orientation.East;
            Rover rover = Rover.CreateAndSendTo(planet, deployLocation, deployOrientation);
            char[] commands = "frff".ToCharArray();
            Point expectedLocationAfterMovement = new(1, 1);
            Orientation expectedOrientationAfterMovement = Orientation.South;

            // Act & Assert
            ObstacleEncounteredException? exc = Assert.Throws<ObstacleEncounteredException>(() =>
            {
                rover.ExecuteCommands(commands);
            });

            Assert.AreEqual(expectedLocationAfterMovement, rover.Location);
            Assert.AreEqual(expectedOrientationAfterMovement, rover.Orientation);
            Assert.AreEqual(exc?.ObstacleLocation, obstacleLocation);
        }
    }
}