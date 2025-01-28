namespace RYTM_Test_PashininVadim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var fileInput = string.Empty;
            var fileOutput = string.Empty;
            if (args.Length == 2)
            {
                fileInput = args[0];
                fileOutput = args[1];
            }
            else
            {
                Console.WriteLine("Введите имена файла с задачей и файла для записей результата:");
                fileInput = Console.ReadLine();
                fileOutput = Console.ReadLine();
            }
            var mg = new MetaGraph();
            mg.FormGraph(fileInput);
            mg.WriteGraph(fileOutput);
        }
    }
}
