using System.Drawing;
using MarsRover.Api.Models;

namespace MarsRover.Console.Models
{
    public record CommandResult(int Index, bool Successful, Point Location, Orientation Orientation)
    {
    };
}
