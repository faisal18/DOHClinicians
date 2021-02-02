namespace DOHClinicians
{
    class Program
    {
        static void Main(string[] args)
        {

            //Clinicians->Download & Convert
            //History->Download & Convert
            //GetActive(Clinicians, History)
            //CreateOutput(ActiveOnly)
            //ParseThroughLMU->GateLogic & Speciality
            //UploadOnLMU

            DOH obj = new DOH();
            obj.Controller();
        }
    }
}
