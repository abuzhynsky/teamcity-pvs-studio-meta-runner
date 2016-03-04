namespace TeamCity.PvsStudio.MetaRunner.Tests.Helpers
{
    public class PvsStudioExpectedErrorCode
    {
        public PvsStudioExpectedErrorCode(string errorCode, string category, int priority, int occurrenceCount)
        {
            ErrorCode = errorCode;
            Category = category;
            OccurrenceCount = occurrenceCount;
            Priority = priority;
        }

        public string ErrorCode { get; private set; }

        public string Category { get; private set; }

        public int OccurrenceCount { get; private set; }

        public int Priority { get; private set; }
    }
}