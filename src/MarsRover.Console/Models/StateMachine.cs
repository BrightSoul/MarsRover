using System;
using System.Drawing;
using System.Threading;
using MarsRover.Api.Models;
using static System.Console;

namespace MarsRover.Console.Models
{
    internal static class StateMachine
    {
        public static void Run(Planet planet)
        {
            StateMachineContext context = new("MARS", planet, State: InputLandingLocation)
            {
                Location = new(planet.Size.Width / 2, planet.Size.Height / 2),
                Orientation = Orientation.North
            };

            Clear();

            // Render loop
            while (true)
            {
                CursorVisible = false;
                SetCursorPosition(0, 0);
                RenderPlanet(context);
                Func<StateMachineContext, StateMachineContext> handleState = context.State;
                context = handleState(context);
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
                        if (context.State == InputLandingLocation || context.State == InputLandingOrientation)
                        {
                            BackgroundColor = hasObstacle ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                        }

                        if (context.State != InputLandingLocation)
                        {
                            square = context.Orientation switch
                            {
                                Orientation.North => '^',
                                Orientation.East => '>',
                                Orientation.West => '<',
                                Orientation.South => 'v',
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

        private static StateMachineContext InputLandingLocation(StateMachineContext context)
        {
            PromptInputLandingLocation(context);
            ConsoleKeyInfo keyInfo = ReadKey(intercept: true);

            return context with
            {
                Location = keyInfo.Key switch
                {
                    ConsoleKey.UpArrow when context.Location.Y > 0 => new Point(context.Location.X, context.Location.Y - 1),
                    ConsoleKey.DownArrow when context.Location.Y < context.Planet.Size.Height - 1 => new Point(context.Location.X, context.Location.Y + 1),
                    ConsoleKey.LeftArrow when context.Location.X > 0 => new Point(context.Location.X - 1, context.Location.Y),
                    ConsoleKey.RightArrow when context.Location.X < context.Planet.Size.Width - 1 => new Point(context.Location.X + 1, context.Location.Y),
                    _ => context.Location
                },

                State = keyInfo.Key switch
                {
                    ConsoleKey.Enter when !context.Planet.HasObstacleAt(context.Location) => InputLandingOrientation,
                    _ => InputLandingLocation
                }
            };
        }

        private static void PromptInputLandingLocation(StateMachineContext context)
        {
            Write($"Use arrow keys to select a landing location: ");
            bool hasObstacle = context.Planet.HasObstacleAt(context.Location);
            ForegroundColor = hasObstacle ? ConsoleColor.Red : ConsoleColor.Green;
            Write(context.Location);
            ForegroundColor = ConsoleColor.DarkGray;
            Write(" ");
            Write(hasObstacle ? "Can't land on obstacle" : "Press ENTER to confirm");
            WriteLineSpaceFill();
            ResetColor();
        }

        private static StateMachineContext InputLandingOrientation(StateMachineContext context)
        {
            PromptInputSendOrientation(context);
            ConsoleKeyInfo keyInfo = ReadKey(intercept: true);

            return context with
            {
                Orientation = keyInfo.Key switch
                {
                    ConsoleKey.UpArrow => Orientation.North,
                    ConsoleKey.DownArrow => Orientation.South,
                    ConsoleKey.LeftArrow => Orientation.West,
                    ConsoleKey.RightArrow => Orientation.East,
                    _ => context.Orientation
                },

                State = keyInfo.Key switch
                {
                    ConsoleKey.Enter => SendRover,
                    _ => InputLandingOrientation
                }
            };
        }

        private static void PromptInputSendOrientation(StateMachineContext context)
        {
            Write($"Use arrow keys to select landing orientation: ");
            ForegroundColor = ConsoleColor.Green;
            Write(context.Orientation);
            ForegroundColor = ConsoleColor.DarkGray;
            Write(" ");
            Write("Press ENTER to confirm");
            WriteLineSpaceFill();
            ResetColor();
        }

        private static StateMachineContext InputCommands(StateMachineContext context)
        {
            PromptInputCommands(context);
            ConsoleKeyInfo keyInfo = ReadKey(intercept: true);

            return context with
            {
                Commands = keyInfo.Key switch
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
                },

                State = keyInfo.Key switch
                {
                    ConsoleKey.Enter or ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.UpArrow or ConsoleKey.DownArrow => ExecuteCommands,
                    _ => InputCommands
                }
            };
        }

        private static void PromptInputCommands(StateMachineContext context)
        {
            PromptInputCommandsHeader(context);
            ForegroundColor = ConsoleColor.White;
            Write(context.Commands);
            (int left, int top) = GetCursorPosition();
            ForegroundColor = ConsoleColor.DarkGray;
            Write("  ");
            Write((context.Commands.Length > 0 ? "Press ENTER to confirm" : "Any of f,b,l,r"));
            WriteLineSpaceFill();
            ResetColor();
            SetCursorPosition(left, top);
            CursorVisible = true;
        }

        private static void PromptInputCommandsHeader(StateMachineContext context)
        {
            Write($"Rover at {{X={context.Location.X},Y={context.Location.Y},{context.Orientation}}}. Input one or more commands: ");
        }

        private static StateMachineContext SendRover(StateMachineContext context)
        {
            return context with
            {
                Rover = Rover.CreateAndSendTo(context.Planet, context.Location, context.Orientation),
                State = InputCommands
            };
        }

        private static StateMachineContext ExecuteCommands(StateMachineContext context)
        {
            if (context.Rover == null)
            {
                throw new InvalidOperationException("Rover is not null");
            }
            
            context.RenderQueue.Clear();
            Rover rover = context.Rover;

            void ReportCommandResult(bool isSuccessful, Point location, Orientation orientation)
            {
                int commandIndex = context.RenderQueue.Count;
                CommandResult commandResult = new(commandIndex, isSuccessful, location, orientation);
                context.RenderQueue.Enqueue(commandResult);
            };

            void HandleMoved(object? sender, MovedEventArgs args) => ReportCommandResult(isSuccessful: true, args.Location, args.Orientation);

            rover.Moved += HandleMoved;
            
            try
            {
                char[] commandsArray = context.Commands.ToCharArray();
                rover.ExecuteCommands(commandsArray);
            }
            catch (ObstacleEncounteredException exc)
            {
                ReportCommandResult(isSuccessful: false, exc.ObstacleLocation, rover.Orientation);
            }
            finally
            {
                rover.Moved -= HandleMoved;
            }

            return context with
            {
                State = RenderMovement
            };
        }

        private static StateMachineContext RenderMovement(StateMachineContext context)
        {
            // Simulate delay in command execution to give the user a chance to see what's happening
            Thread.Sleep(300);

            if (context.RenderQueue.Count == 0)
            {
                // We rendered every result, start over with a new batch of commands
                return context with
                {
                    Commands = string.Empty,
                    State = InputCommands
                };
            }

            CommandResult result = context.RenderQueue.Dequeue();
            if (result.IsSuccessful)
            {
                context = context with
                {
                    Location = result.Location,
                    Orientation = result.Orientation,
                };
            }

            DisplayCommandResult(context, result);
            return context;
        }

        private static void DisplayCommandResult(StateMachineContext context, CommandResult result)
        {
            PromptInputCommandsHeader(context);

            for (int i = 0; i < context.Commands.Length; i++)
            {
                if (i < result.Index || (i == result.Index && result.IsSuccessful))
                {
                    BackgroundColor = ConsoleColor.DarkGreen;
                }
                else if (i == result.Index && !result.IsSuccessful)
                {
                    BackgroundColor = ConsoleColor.DarkRed;
                }

                Write(context.Commands[i]);
                ResetColor();
            }

            if (!result.IsSuccessful)
            {
                ForegroundColor = ConsoleColor.DarkRed;
                Write(" ");
                Write($"Obstacle encountered at {result.Location}");
                ResetColor();
                WriteLineSpaceFill();
                Thread.Sleep(1000);
            }
            else
            {
                Write(" ");
                if (result.Index == context.Commands.Length - 1)
                {
                    ForegroundColor = ConsoleColor.Green;
                    Write("Success!");
                }
                else
                {
                    ForegroundColor = ConsoleColor.DarkGray;
                    Write("Executing...");
                }

                ResetColor();
                WriteLineSpaceFill();
            }
        }

        private static void WriteLineSpaceFill()
        {
            (int left, int _) = GetCursorPosition();
            WriteLine(string.Empty.PadLeft(Math.Max(0, WindowWidth - left), ' '));
        }
    }
}
