/*
 *  test password verification, stream read/write
 */

using System;

using CipherBox.Zip;

namespace CipherBox.Zip.Test
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Help:");
                Console.WriteLine(" program show   zipfile          - show enc info");
                Console.WriteLine(" program verify zipfile password - verify password");
                Console.WriteLine(" program lock   zipfile password - lock file");
                Console.WriteLine(" program unlock zipfile password - unlock file");
                Console.WriteLine(" program zip    file    password - zip file");
                Console.WriteLine(" program unzip  zipfile password - unzip file");
                return;
            }

            string cmd = null;
            string filename = null;
            string password = null;
            if (args.Length > 0) { cmd = args[0]; }
            if (args.Length > 1) { filename = args[1]; }
            if (args.Length > 2) { password = args[2]; }


            if (args.Length == 2 && cmd == "show")
            {
                string dis = ZipHelper.GetEncryptionInfo(filename);
                Console.WriteLine(dis);
                return;
            }

            if (args.Length == 3 && cmd == "verify")
            {
                if (ZipHelper.VerifyPassword(filename, password))
                {
                    Console.WriteLine("success");                    
                }
                else
                {
                    Console.WriteLine("fail");
                }
                return;
            }

            if (args.Length == 3 && cmd == "lock")
            {
                ZipHelper.AddPassword(filename, password);
            }

            if (args.Length == 3 && cmd == "unlock")
            {
                ZipHelper.RemovePassword(filename, password);
            }

            if (args.Length >=2 && cmd == "zip")
            {
                ZipHelper.Compress(filename, password);
            }

            if (args.Length >= 2 && cmd == "unzip")
            {
                ZipHelper.Extract(filename, password);
            }

        }


    }
}
