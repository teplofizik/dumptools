using NandDump.Core;
using NandDump.Reader;
using System;
using System.IO;

namespace NandDump
{
    class Program
    {
        static Config Cfg = new Config();
        static SerialReader serial;
        static UbootReader uboot;

        static void DumpNandToFile(uint Flash, uint Size, string FN, bool Verbose)
        {
            var Dump = uboot.GetNandDump(Cfg.Ram, Flash, Size, Verbose, Cfg.FFStop);

            // File.WriteAllLines(FN, Dump);

            var Bytes = UbootReader.ConvertToBytes(Dump);
            File.WriteAllBytes($"{FN}", Bytes);
        }

        static void DumpFromFile(string Source, string To)
        {
            var Lines = File.ReadAllLines(Source);

            var Bytes = UbootReader.ConvertToBytes(Lines);
            File.WriteAllBytes(To, Bytes);
        }

        static void DumpAll()
        {
            uboot.Start();

            //

            //  DumpNandToFile(0x260000, 0x20000, "0x260000_20000.txt", true); // /dev/mtd6 am335x-boneblack-bitmainer.dtb
            //  DumpNandToFile(0x800000, 0x1400000, "0x800000_1400000.txt", true);
            //  DumpNandToFile(0x280000, 0x500000, "0x280000_500000.txt", true); // /dev/mtd7 uImage.bin
            //  DumpNandToFile(0x800000, 0x2000, "0x800000_2000.txt", true);
            DumpNandToFile(0xfeaf000, 0x40000+0xb0000, "0x2000000_2000.txt", true);
            uboot.Stop();
        }

        static void DumpByArgs(string[] args)
        {
            var Offset = Convert.ToUInt32(args[2], 16);
            var Size = Convert.ToUInt32(args[3], 16);
            var FN = args[4];
            var Verbose = Convert.ToUInt32(args[5]) != 0;

            uboot.Start();
            DateTime Started = DateTime.Now;
            DumpNandToFile(Offset, Size, FN, Verbose);

            var Elapsed = DateTime.Now - Started;
            Console.WriteLine($"Elapsed {Elapsed}");
            uboot.Stop();
        }


        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var From = args[0];
                var To = args[1];
                DumpFromFile(From, To);

                return;
            }

            if (File.Exists("config.txt"))
            {
                Cfg.Load("config.txt");
            }

            if (args.Length == 6)
            {
                serial = new SerialReader(args[0], Convert.ToInt32(args[1]));
                uboot = new UbootReader(serial);

                DumpByArgs(args);
            }
            else
            {
                // USAGE:
                Console.WriteLine("Usage: NandDump <Port> <Baud> <Offset> <Size> <Filename> <Verbose>");
                Console.WriteLine("Usage: NandDump COM1 115200 800000 20000 0x260000_20000.bin 1");
            }
        }
    }
}
