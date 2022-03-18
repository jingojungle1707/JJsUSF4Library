using JJsUSF4Library;
using JJsUSF4Library.FileClasses;
using JJsUSF4Library.FileClasses.ScriptClasses;
using JJsUSF4Library.FileClasses.SubfileClasses;
using System.IO;
using System;

namespace JJsUSF4LibraryTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter filepath to load a USF4 file:");
                string filePath = Console.ReadLine();

                if (File.Exists(Path.GetFullPath(filePath)))
                {
                    USF4File usf4File = USF4Utils.OpenFileStreamCheckCompression(filePath);
                    Console.WriteLine($"Opened file {usf4File.Name}");
                    Console.WriteLine($"File parsed as {usf4File.GetType()}.");
                    if (usf4File.GetType() == typeof(OtherFile))
                    {
                        Console.WriteLine("'OtherFile' may indicate this is not a recognised USF4 file type, or that this filetype is not yet implemented.");
                    }
                    Console.WriteLine(" ");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();

                    return;
                }
                else Console.WriteLine("Invalid path.");
            }
        }
    }
}
