using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MarsRover.Api.Models;
using NUnit.Framework;

namespace MarsRover.Api.Test
{
    public class PlanetTests
    {
        [Test]
        [TestCase(1.0, true)]
        [TestCase(0.0, false)]
        public void HasObstacleAt_ReturnsCorrectResult(double obstaclesRatio, bool expectedResult)
        {
            // Arrange
            Size size = new(1, 1);
            Planet planet = new(size, obstaclesRatio);
            Point point = new(0, 0);

            // Act
            bool actualResult = planet.HasObstacleAt(point);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(2, 3)]
        [TestCase(2, 2)]
        [TestCase(1, 3)]
        public void HasObstacleAt_ThrowsException_WhenCoordinateIsOutOfBounds(int x, int y)
        {
            // Arrange
            Size size = new(2, 3);
            double obstaclesRatio = 0D;
            Planet planet = new(size, obstaclesRatio);
            Point point = new(x, y);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
            {
                planet.HasObstacleAt(point);
            });
        }
    }
}