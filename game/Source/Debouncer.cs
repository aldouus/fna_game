public class Debouncer
{
    private double lastTime;
    private readonly double interval;

    public Debouncer(double intervalMilliseconds)
    {
        interval = intervalMilliseconds;
        lastTime = double.MinValue;
    }

    public bool CanTrigger(double currentTime)
    {
        if (currentTime - lastTime > interval)
        {
            lastTime = currentTime;
            return true;
        }
        return false;
    }
}
