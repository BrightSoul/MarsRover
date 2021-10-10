using System;
using System.Drawing;
using System.Threading;
using MarsRover.Api.Models;
using MarsRover.Console.Models;
using static System.Console;

namespace MarsRover.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new();
            Size size = new(25, 10);
            Planet mars = Planet.CreateWithRandomlyGeneratedObstacles(size);
            StateMachineContext context = new("MARS", mars)
            {
                State = MachineState.InputSendLocation,
                Location = new(size.Width / 2, size.Height / 2),
                Orientation = Orientation.North
            };
            
            Clear();

            while (!tokenSource.Token.IsCancellationRequested)
            {
                CursorVisible = false;
                SetCursorPosition(0, 0);
                RenderPlanet(context);
                context.State = context.State switch
                {
                    MachineState.InputSendLocation => InputSendLocation(context),
                    MachineState.InputSendOrientation => InputSendOrientation(context),
                    MachineState.SendRover => SendRover(context),
                    MachineState.InputCommands => InputCommands(context),
                    MachineState.ExecuteCommands => ExecuteCommands(context),
                    MachineState.RenderMovement => RenderMovement(context),
                    _ => throw new NotSupportedException("State not supported")
                };
            }
        }

        private static void RenderPlanet(StateMachineContext context)
        {
            WriteLine('┌' + context.Title.PadRight(context.Planet.Size.Width, '─') + "┐");
            for (int y = 0; y < context.Planet.Size.Height; y++)
            {
                Write('│');
                for (int x = 0; x < context.Planet.Size.Width; x++)
                {
                    Point location = new(x, y);
                    bool hasObstacle = context.Planet.HasObstacleAt(location);
                    char square = hasObstacle ? '#' : ' ';
                    if (location == context.Location)
                    {
                        if (context.State == MachineState.InputSendLocation || context.State == MachineState.InputSendOrientation)
                        {
                            BackgroundColor = hasObstacle ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                        }

                        if (context.State != MachineState.InputSendLocation)
                        {
                            square = context.Orientation switch
                            {
                                Orientation.North => '↑',
                                Orientation.East => '→',
                                Orientation.West => '←',
                                Orientation.South => '↓',
                                _ => square
                            };
                        }

                        Write(square);
                        ResetColor();
                    }
                    else
                    {
                        Write(square);
                    }
                }

                WriteLine('│');
            }

            WriteLine('└' + "".PadLeft(context.Planet.Size.Width, '─') + "┘");
        }

        private static MachineState InputSendLocation(StateMachineContext context)
        {
            Write($"Use arrow keys to choose deploy location: ");
            bool hasObstacle = context.Planet.HasObstacleAt(context.Location);
            ForegroundColor = hasObstacle ? ConsoleColor.Red : ConsoleColor.Green;
            Write(context.Location);
            ForegroundColor = ConsoleColor.DarkGray;
            Write(" ");
            Write(hasObstacle ? "Can't deploy on obstacle" : "Press ENTER to confirm");
            WritePadding();
            ResetColor();

            ConsoleKeyInfo key = ReadKey();
            context.Location = key.Key switch
            {
                ConsoleKey.UpArrow when context.Location.Y > 0 => new Point(context.Location.X, context.Location.Y - 1),
                ConsoleKey.DownArrow when context.Location.Y < context.Planet.Size.Height - 1 => new Point(context.Location.X, context.Location.Y + 1),
                ConsoleKey.LeftArrow when context.Location.X > 0 => new Point(context.Location.X - 1, context.Location.Y),
                ConsoleKey.RightArrow when context.Location.X < context.Planet.Size.Width - 1 => new Point(context.Location.X + 1, context.Location.Y),
                _ => context.Location
            };

            return key.Key switch
            {
                ConsoleKey.Enter when !context.Planet.HasObstacleAt(context.Location) => MachineState.InputSendOrientation,
                _ => MachineState.InputSendLocation
            };
        }

        private static MachineState InputSendOrientation(StateMachineContext context)
        {
            Write($"Use arrow keys to choose deploy orientation: ");
            ForegroundColor = ConsoleColor.Green;
            Write(context.Orientation);
            ForegroundColor = ConsoleColor.DarkGray;
            Write(" ");
            Write("Press ENTER to confirm");
            WritePadding();
            ResetColor();

            ConsoleKeyInfo keyInfo = ReadKey();
            context.Orientation = keyInfo.Key switch
            {
                ConsoleKey.UpArrow => Orientation.North,
                ConsoleKey.DownArrow => Orientation.South,
                ConsoleKey.LeftArrow => Orientation.West,
                ConsoleKey.RightArrow => Orientation.East,
                _ => context.Orientation
            };

            return keyInfo.Key switch
            {
                ConsoleKey.Enter => MachineState.SendRover,
                _ => MachineState.InputSendOrientation
            };
        }

        private static MachineState InputCommands(StateMachineContext context)
        {
            Write($"Input one or more commands: ");
            ForegroundColor = ConsoleColor.White;
            Write(context.Commands);
            (int left, int top) = GetCursorPosition();
            ForegroundColor = ConsoleColor.DarkGray;
            Write("  ");
            Write((context.Commands.Length > 0 ? "Press ENTER to confirm" : "Any of f,b,l,r"));
            WritePadding();
            ResetColor();

            SetCursorPosition(left, top);
            CursorVisible = true;
            ConsoleKeyInfo keyInfo = ReadKey();
            context.Commands = keyInfo.Key switch
            {
                ConsoleKey.L => $"{context.Commands}l",
                ConsoleKey.R => $"{context.Commands}r",
                ConsoleKey.F => $"{context.Commands}f",
                ConsoleKey.B => $"{context.Commands}b",
                ConsoleKey.LeftArrow => "l",
                ConsoleKey.RightArrow => "r",
                ConsoleKey.UpArrow => "f",
                ConsoleKey.DownArrow => "b",
                ConsoleKey.Backspace => context.Commands.Substring(0, Math.Max(0, context.Commands.Length - 1)),
                _ => context.Commands
            };

            return keyInfo.Key switch
            {
                ConsoleKey.Enter or ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.UpArrow or ConsoleKey.DownArrow => MachineState.ExecuteCommands,
                _ => MachineState.InputCommands
            };
        }

        private static MachineState SendRover(StateMachineContext context)
        {
            context.Rover = Rover.CreateAndSendTo(context.Planet, context.Location, context.Orientation);
            return MachineState.InputCommands;
        }

        private static MachineState ExecuteCommands(StateMachineContext context)
        {
            context.RenderQueue.Clear();
            Rover rover = context.Rover!;

            void ReportCommandResult(bool successful, Point location, Orientation orientation)
            {
                int commandIndex = context.RenderQueue.Count;
                CommandResult commandResult = new(commandIndex, successful, location, orientation);
                context.RenderQueue.Enqueue(commandResult);
            };

            void HandleMoved(object? sender, MovedEventArgs args) => ReportCommandResult(successful: true, args.Location, args.Orientation);

            rover.Moved += HandleMoved;
            try
            {
                char[] commandsArray = context.Commands.ToCharArray();
                rover.ExecuteCommands(commandsArray);
            }
            catch
            {
                ReportCommandResult(successful: false, rover.Location, rover.Orientation);
            }
            finally
            {
                rover.Moved -= HandleMoved;
            }

            return MachineState.RenderMovement;
        }

        private static MachineState RenderMovement(StateMachineContext context)
        {
            Thread.Sleep(300);
            if (context.RenderQueue.Count == 0)
            {
                context.Commands = string.Empty;
                return MachineState.InputCommands;
            }

            Write($"Input one or more commands: ");
            CommandResult result = context.RenderQueue.Dequeue();
            context.Location = result.Location;
            context.Orientation = result.Orientation;

            for (int i = 0; i < context.Commands.Length; i++)
            {
                if (i < result.Index || (i == result.Index && result.Successful))
                {
                    BackgroundColor = ConsoleColor.DarkGreen;
                }
                else if (i == result.Index && !result.Successful)
                {
                    BackgroundColor = ConsoleColor.DarkRed;
                }

                Write(context.Commands[i]);
                ResetColor();
            }

            if (!result.Successful)
            {
                ForegroundColor = ConsoleColor.DarkRed;
                Write(" ");
                Write($"Obstacle encountered at {result.Location}");
                ResetColor();
                WritePadding();
                Thread.Sleep(1000);
            }
            else
            {
                Write(" ");
                if (result.Index == context.Commands.Length - 1)
                {
                    ForegroundColor = ConsoleColor.Green;
                    Write("Successful!");
                }
                else
                {
                    ForegroundColor = ConsoleColor.DarkGray;
                    Write("Executing...");
                }
                
                ResetColor();
                WritePadding();
            }

            // Give the user a chance to see each command executing
            return MachineState.RenderMovement;
        }

        private static void WritePadding()
        {
            (int left, int _) = GetCursorPosition();
            WriteLine(string.Empty.PadLeft(Math.Max(0, WindowWidth - left), ' '));
        }
    }
}
