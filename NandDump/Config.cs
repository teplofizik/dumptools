using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NandDump
{
    class Config
    {
        public UInt64 Ram = 0;
        public int FFStop = 0;

        public void Load(string Filename)
        {
            try
            {
                var Lines = File.ReadAllLines(Filename);
                var Sep = new char[] { '=' };

                foreach (var L in Lines)
                {
                    var Parts = L.Split(Sep);
                    if(Parts.Length == 2)
                    {
                        var Name = Parts[0];
                        var Value = Parts[1];

                        switch(Name)
                        {
                            case "ram": 
                                Ram = Convert.ToUInt64(Value, 16);
                                Console.WriteLine($"Ram start address: 0x{Ram:X8}");
                                break;
                            case "ffstop": 
                                FFStop = Convert.ToInt32(Value);
                                Console.WriteLine($"Stop after {FFStop} empty lines");
                                break;
                        }
                    }
                }

            }
            catch(Exception E)
            {
                Console.WriteLine("Config parse error: " + E.Message);
            }
        }
    }
}
