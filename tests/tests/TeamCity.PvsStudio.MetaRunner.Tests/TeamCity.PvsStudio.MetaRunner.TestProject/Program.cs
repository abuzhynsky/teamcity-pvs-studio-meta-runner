namespace TeamCity.PvsStudio.MetaRunner.TestProject
{
    static class Program
    {
        static void Main()
        {
            var s = "qwerty";

            //issue is made intentionally for testing PVS Studio
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            s.Replace("q", string.Empty);
        }
    }
}
