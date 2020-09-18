using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Vishnu.Interchange;

namespace TextFileLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            TextFileLogger cl = new TextFileLogger();
            cl.Log(null, new TreeParameters("Test-Tree", null),
              new TreeEvent("wasPassiert auf standard LogPath", "4711", "4712", "ne Knoden", "", false, NodeLogicalState.Done, null, null), null);
            cl.Log(@"sub1\TextFileLoggerDemo.log", new TreeParameters("Test-Tree", null),
              new TreeEvent("wasPassiert auf speziellem LogPath", "4711", "4712", "ne Knoden", "", false, NodeLogicalState.Done, null, null), null);
        }
    }
}
