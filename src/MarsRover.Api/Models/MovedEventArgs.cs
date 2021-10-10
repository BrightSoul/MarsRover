using System.Drawing;

namespace MarsRover.Api.Models
{
    public record MovedEventArgs(Point Location, Orientation Orientation);
}