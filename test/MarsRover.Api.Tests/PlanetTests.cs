using System;
using System.Drawing;
using MarsRover.Api.Models;
using NUnit.Framework;

namespace MarsRover.Api.Test
{
    public class PlanetTests
    {
        [Test]
        [TestCase(0, 0, false)]
        [TestCase(1, 0, true)]
        public void HasObstacleAt_ReturnsCorrectResult(int checkLocationX, int checkLocationY, bool expectToEncounterObstacle)
        {
            // Arrange
            Size size = new(2, 1);
            Point[] obstaclesLocations = new [] { new Point(1, 0) };
            Planet planet = Planet.CreateWithGivenObstacles(size, obstaclesLocations);
            Point pointToCheck = new(checkLocationX, checkLocationY);

            // Act
            bool actuallyEncounteredObstacle = planet.HasObstacleAt(pointToCheck);

            // Assert
            Assert.AreEqual(expectToEncounterObstacle, actuallyEncounteredObstacle);
        }

        [Test]
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(2, 3)]
        [TestCase(2, 2)]
        [TestCase(1, 3)]
        public void HasObstacleAt_ThrowsException_WhenLocationIsNotWithinSurface(int locationX, int locationY)
        {
            // Arrange
            Size size = new(2, 3);
            Planet planet = Planet.CreateEmpty(size);
            Point point = new(locationX, locationY);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                planet.HasObstacleAt(point);
            });
        }

        [Test]
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 0)]
        public void CreateEmpty_ThrowsException_WhenSizeIsInvalid(int width, int height)
        {
            // Arrange
            Size size = new(width, height);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                Planet.CreateEmpty(size);
            });
        }

        [Test]
        [TestCase(-1, 0, 0.0)] // Invalid size, valid ratio
        [TestCase(0, -1, 0.0)] // Invalid size, valid ratio
        [TestCase(0, 0, 0.0)]  // Invalid size, valid ratio
        [TestCase(1, 1, -1.0)] // Valid size, invalid ratio
        [TestCase(1, 1, 1.1)]  // Valid size, invalid ratio
        public void CreateWithRandomlyGeneratedOnstacles_ThrowsException_WhenSizeObstaclesRatioOrAreInvalid(int width, int height, double obstaclesToEmptySpaceRatio)
        {
            // Arrange
            Size size = new(width, height);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                Planet.CreateWithRandomlyGeneratedObstacles(size, obstaclesToEmptySpaceRatio);
            });
        }

        [Test]
        [TestCase(0.0, false)] // Invalid size, valid ratio
        [TestCase(1.0, true)] // Invalid size, valid ratio
        public void CreateWithRandomlyGeneratedOnstacles_PlacesObstaclesAccordingToRatio(double obstaclesToEmptySpaceRatio, bool expectToEncounterObstacle)
        {
            // Arrange
            Size size = new(1, 1);
            Planet planet = Planet.CreateWithRandomlyGeneratedObstacles(size, obstaclesToEmptySpaceRatio);
            Point checkLocation = new Point(0, 0);

            // Act
            bool actuallyEncounteredObstacle = planet.HasObstacleAt(checkLocation);
            
            // Assert
            Assert.AreEqual(expectToEncounterObstacle, actuallyEncounteredObstacle);
        }
    }
}