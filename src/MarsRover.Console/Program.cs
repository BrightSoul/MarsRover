using System.Drawing;
using MarsRover.Api.Models;
using MarsRover.Console.Models;

namespace MarsRover.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Size size = new(25, 10); // TODO: Optionally get this from args
            Planet mars = Planet.CreateWithRandomlyGeneratedObstacles(size);
            StateMachine.Run(mars);
        }
    }
}
