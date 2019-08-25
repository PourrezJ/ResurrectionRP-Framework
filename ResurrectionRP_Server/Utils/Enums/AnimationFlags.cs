

namespace ResurrectionRP_Server.Utils.Enums
{
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7,
        UpperBodyOnly = 1 << 16,
        SecondaryTask = 1 << 32
    }
}
