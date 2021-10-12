using System;
using System.Collections.Generic;
using System.Drawing;
using MarsRover.Api.Models;

namespace MarsRover.Console.Models
{
    public record StateMachineContext(string Title, Planet Planet, Func<StateMachineContext, StateMachineContext> State)
    {
        public Rover? Rover { get; init; }
        public Point Location { get; init; }
        public Orientation Orientation { get; init; }
        public string Commands { get; init; } = string.Empty;
        public Queue<CommandResult> RenderQueue { get; } = new();
    }
}
