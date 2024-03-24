using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        Queue<string> q = new Queue<string>();

        public void test()
        {
            string str = q.Dequeue();
            Console.WriteLine(str);
        }

        public void testArg()
        {
            String key = "main(123,123123)";
            Regex r = new Regex(@"\((.*)\)");

            Match match = r.Match(key);
            if (match.Success) 

            key = Regex.Replace(key, r.ToString(), "()");
            Console.WriteLine(key);
        }

        public void testRegex()
        {
            string name = "金币 (2,000)";
            name = Regex.Replace(name, @"\([\d,]+\)", string.Empty);
            Console.WriteLine(name);
        }

        public void testCode()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = true; // it's still going to generate a file somewhere in AppData (temp)
            parameters.TreatWarningsAsErrors = false;

            // I need these references because the program that I will 'secure'
            // is that Form from the photo above that requires a password
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");

            // getContents() is a method that extracts & decrypts the source codes 
            // of the 'secured' application and returns everything as an array of Strings
            // in order to be compiled

          //  CompilerResults result = provider.CompileAssemblyFromSource(parameters, getContents());
        }
        
        public void testNewLine()
        {
            string str = "a\nb";
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == '\n')
                    Console.WriteLine("find: " + i);
            }
        }

        public void testMemoryStream()
        {
            byte[] rawBytes = new byte[4];
            rawBytes[0] = 4;
            rawBytes[1] = 0;
            if (rawBytes.Length < 4) return; //| 2Bytes: Packet Size | 2Bytes: Packet ID |

            int length = (rawBytes[1] << 8) + rawBytes[0];

            if (length > rawBytes.Length || length < 2) return;

            short id = 0;
            using (MemoryStream stream = new MemoryStream(rawBytes, 2, length - 2))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                id = reader.ReadInt16();
                Console.WriteLine(id);
            };
        }

        public void testSwith()
        {
            string part1 = "G_DOGYOARENA";
            Match match = Regex.Match(part1, @"[A-Z_0-9]+", RegexOptions.IgnoreCase);
            if (match.Success)
                Console.WriteLine("aaaaaaa");
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.testSwith();
            //  p.test();

            // p.testRegex();

            //p.testCode();

            //  p.testNewLine();

            // Console.WriteLine(p.p.X + p.p.Y);

            //MirHttpSrv httpSrv = new MirHttpSrv();

            //httpSrv.Setup();
        }
    }
}
