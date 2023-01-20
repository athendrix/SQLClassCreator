using System;
using System.Linq;
using System.Collections.Generic;
using CSL.SQL.ClassCreator;
using System.IO;

namespace SQLClassCreator
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && Directory.Exists(args[0]))
            {
                FileConverter.Convert(new DirectoryInfo(args[0]));
            }
            else if (args != null && args.Length > 0 && File.Exists(args[0]))
            {
                FileConverter.Convert(new FileInfo(args[0]));
            }
        }
    }
}
