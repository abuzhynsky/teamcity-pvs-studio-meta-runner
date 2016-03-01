namespace TeamCity.PvsStudio.MetaRunner.Tests.Helpers
{
    public class PvsStudioExpectedErrorCode
    {
        public PvsStudioExpectedErrorCode(string errorCode, int occurrenceCount)
        {
            ErrorCode = errorCode;
            OccurrenceCount = occurrenceCount;
        }

        public string ErrorCode { get; private set; }

        public int OccurrenceCount { get; private set; }
    }
}