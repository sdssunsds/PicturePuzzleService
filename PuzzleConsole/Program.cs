using PicturePuzzleService;
using System;
using System.Threading;

namespace PuzzleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            PicService.EnginePath = Properties.Settings.Default.EnginePath;
            PicService.UploadPath = Properties.Settings.Default.UploadPath;
            PicService.PuzzlePath = Properties.Settings.Default.PuzzlePath;
            PicService.BakPath = Properties.Settings.Default.BakPath;
            PicService.ZipPath = Properties.Settings.Default.ZipPath;
            PicService service = new PicService();
            Console.WriteLine($"ID:{args[0]}\tRobot:{args[1]}\t车厢号:{args[2]}");
            service.Complete(args[0], args[1], int.Parse(args[2]));
            while (!service.complete1 || !service.complete2 || !service.complete3)
            {
                Thread.Sleep(3000);
            }
            Console.ReadKey();
        }
    }
}
