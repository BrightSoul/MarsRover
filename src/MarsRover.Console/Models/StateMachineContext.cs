using System.Collections.Generic;
using System.Drawing;
using MarsRover.Api.Models;

namespace MarsRover.Console.Models
{
    public class StateMachineContext
    {
        public StateMachineContext(string title, Planet planet)
        {
            Title = title;
            Planet = planet;
        }

        public string Title { get; }
        public Planet Planet { get; }
        public MachineState State { get; set; }
        public Point Location { get; set; }
        public Orientation Orientation { get; set; }
        public string Commands = string.Empty;
        public Rover? Rover { get; set; } = null;
        public Queue<CommandResult> RenderQueue { get; } = new();
    }
}
