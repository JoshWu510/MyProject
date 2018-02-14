using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace QCI_VR
{
    class Program
    {
        static void Main(string[] args)
        {
            string arguments;
            string resourceName = "Tool";
            if (args.Length > 0)
            {
                arguments = args[0].ToString().ToLower();
                switch (arguments)
                {
                    case "-v":
                        ExtractResource(resourceName, "/GetDeviceInfo", "null");
                        break;
                    case "-f":
                        if (args.Length != 2)
                        {
                            Console.WriteLine("-f <app.img>");
                            Environment.Exit(0);
                        }
                        else
                        {
                            string app = args[1].ToString().ToLower();

                            if (!System.IO.File.Exists(app))
                            {
                                Console.WriteLine("-f <app.img>");
                                Console.WriteLine("image not found!");
                                Environment.Exit(0);
                            }
                            else
                            {
                                string command = "/Flash";
                                Console.WriteLine(command);
                                try
                                {
                                    ExtractResource(resourceName, command, Path.GetFullPath(app));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                        }
                        break;
                    default:
                        exit();
                        break;
                }
            }
            else
            {
                exit();
            }
        }

        static void ExtractResource(string resName, string cmd, string cmd2)
        {
            // get resource
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            object ob = Properties.Resources.ResourceManager.GetObject(resName, originalCulture);
            byte[] bin = (byte[])ob;
            // load resource into assembly
            Assembly exe = Assembly.Load(bin);
            if (string.Compare(cmd2, "null") == 0)
            {
                // store console output in strWriter
                StringWriter strWriter = new StringWriter();
                Console.SetOut(strWriter);
                // run resource
                exe.EntryPoint.Invoke(null, new object[] { new string[] { cmd } });
                // console output recovery
                TextWriter outputWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(outputWriter);
                // filter message
                string check = "device not found";
                if (strWriter.ToString().IndexOf(check) > 0)
                {
                    Console.WriteLine(check);
                }
                else
                {
                    string version1 = "3BL Firmware Version: ";
                    string version2 = "FPGA Firmware Size:";
                    int idx1 = strWriter.ToString().IndexOf(version1) + 4;
                    int idx2 = strWriter.ToString().IndexOf(version2);
                    Console.WriteLine(idx1);
                    Console.WriteLine(idx2);
                    if (idx1 > 0)
                    {
                        Console.WriteLine(strWriter.ToString().Substring(idx1, idx2 - idx1));
                    }
                }
            }
            else
            {
                exe.EntryPoint.Invoke(null, new object[] { new string[] { cmd, cmd2 } });
            }
        }

        static void exit()
        {
            Console.Write("\nCommands:\n");
            Console.WriteLine("-f <app.img> : Image Upgrade");
            Console.WriteLine("-v           : Read Device version\n");
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
            Environment.Exit(0);
        }
    }
}
