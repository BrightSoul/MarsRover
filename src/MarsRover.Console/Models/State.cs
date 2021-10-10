namespace MarsRover.Console.Models
{
    public enum State
    {
        InputSendLocation,
        InputSendOrientation,
        SendRover,
        InputCommands,
        ExecuteCommands,
        RenderMovement
    }
}
