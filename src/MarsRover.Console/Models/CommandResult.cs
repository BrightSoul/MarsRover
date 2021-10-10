using System.Drawing;
using MarsRover.Api.Models;

namespace MarsRover.Console.Models
{
    public record CommandResult(int Index, bool IsSuccessful, Point Location, Orientation Orientation)
    {
    };
}
