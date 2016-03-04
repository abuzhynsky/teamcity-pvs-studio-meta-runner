namespace TeamCity.PvsStudio.MetaRunner.TestProject
{
    public class Class1
    {
        public void Method()
        {
            //intentionally made mistakes for PVS-Studio testing
            var s = "qwerty";

            string f = null;

            var a = 0;

            if (f == string.Empty || f == string.Empty)
            {
                a = 5;
            }

            s.Replace("s", a.ToString());
        }
    }
}
