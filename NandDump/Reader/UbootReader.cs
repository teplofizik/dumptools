using NandDump.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NandDump.Reader
{
    class UbootReader
    {
        // https://habr.com/ru/post/420895/
        SerialReader Reader;
        // ctrl+c cmd: \x03
        // cmd: setenv baudrate 115200
        // cmd: nand read 80700000 260000 20000
        // cmd: md.b 80700000 20000
        // 8071fff0: ff ff ff ff ff ff ff ff ff ff ff ff ff ff ff ff    ................
        // Last: U-Boot#

        // Zynq>
        public int EarlyStop = 0;

        public UbootReader(SerialReader Reader)
        {
            this.Reader = Reader;
        }


        public void Start()
        {
            Reader.Open();
        }

        public void Stop()
        {
            Reader.Close();
        }

        private bool IsEnded(string S)
        {
            S = S.Trim();
            if (S.Length > 0)
            {
                var Last = S[S.Length - 1];

                return (S.Length < 20) && ((Last == '#') || (Last == '>'));
            }
            else
                return false;
        }

        public static byte[] ConvertToBytes(string[] Lines)
        {
            var Res = new List<byte>();

            var Sep = new char[] { ' ' };
            foreach (var L in Lines)
            {
                var Start = L.IndexOf(':');
                var End = L.IndexOf("    ");
                if((Start > 0) && (End > 0))
                {
                    var Part = L.Substring(Start + 1, End - Start).Trim();

                    var Parts = Part.Split(Sep);
                    foreach(var P in Parts)
                    {
                        if (P.Length > 0)
                        {
                            Res.Add(Convert.ToByte(P, 16));
                        }
                    }
                }
            }

            return Res.ToArray();
        }

        public string[] GetNandDump(UInt64 Memory, uint Flash, uint Size, bool Verbose, int FFStop)
        {
            var ReadCmd = $"nand read {Memory:x} {Flash:x} {Size:x}\r\n";

            EarlyStop = FFStop;
            Reader.Write(ReadCmd);

            bool Correct = false;
            string L = Reader.Read();
            for (int i = 0; i < 100; i++)
            {
                if(Verbose) Console.WriteLine(L);

                if (IsEnded(L) && (L.IndexOf(ReadCmd) < 0)) break;
                if (L.IndexOf("bytes read: OK") >= 0)
                    Correct = true;
                L = Reader.Read();
            }

            if(Correct)
            {
                var Res = new List<string>();
                ReadCmd = $"md.b {Memory:x} {Size:x}\r\n";

                Reader.Write(ReadCmd);
                L = Reader.Read(); // Skip loopback
                if (Verbose) Console.WriteLine(L);
                L = Reader.Read();

                var Sep = ':';
                var LastAddress = Memory + Size - 16;

                int FFCount = 0;
                // Readout
                while (!IsEnded(L))
                {
                    var TL = L.Trim();
                    if (Verbose) Console.WriteLine(L);
                    Res.Add(TL);

                    var Index = L.IndexOf(':');
                    if (Index > 0)
                    {
                        var Address = Convert.ToUInt32(L.Split(Sep)[0], 16);

                        if (Address == LastAddress)
                            break;

                        if(EarlyStop != 0)
                        {
                            if (L.Contains("ff ff ff ff ff ff ff ff ff ff ff ff ff ff ff ff"))
                            {
                                //Console.WriteLine("EMPTY LINE");
                                FFCount++;
                                if (FFCount == EarlyStop)
                                    break;
                            }
                            else
                                FFCount = 0;
                        }
                    }
                    L = Reader.Read();
                }

                return Res.ToArray();
            }

            return new string[] { };
        }
    }
}
