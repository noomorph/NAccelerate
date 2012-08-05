namespace NAccelerate
{
    public class SegmentAwareProgressChangedEventArgs
    {
        public SegmentAwareProgressChangedEventArgs(int progressPercentage)
        {
            this.ProgressPercentage = progressPercentage;
        }

        public int ProgressPercentage { get; private set; }
    }

    public delegate void SegmentAwareProgressChangedEventHandler(object sender, SegmentAwareProgressChangedEventArgs args);
}
