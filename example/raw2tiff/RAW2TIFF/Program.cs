using MaxssauLibraries;


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello main");
        RawData raw_data = new RawData();

        if (args.Length != 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }

            LibRawProcessor raw = new LibRawProcessor(args[0], ref raw_data);

        }
    }
}