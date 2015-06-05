using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace encorer
{
    class Program
    {
        static int Main(string[] args)
        {
            //Aucun argument
            if (args == null || args.Length == 0)
            {
                info();
                return 0;
            }

            //Liste des encodages disponibles
            if (args[0] == "-e")
            {
                if (args.Length != 1)
                {
                    Console.Error.WriteLine("Only one argument if -e");
                    return 1;
                }
                listerEncodages();
                return 0;
            }

            try
            {
                return executer(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.GetType().FullName + " - " + e.Message);
                return 2;
            }
        }

        static int executer(string[] args)
        {

            //Exécution simple
            Encoding source = null;
            Encoding dest = null;
            string repertoireDest = null;
            List<string> chemins = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                        //Encodage source
                        if (source != null)
                        {
                            Console.Error.WriteLine("-s is already specified");
                            return 1;
                        }

                        i++;
                        if (i == args.Length)
                        {
                            Console.Error.WriteLine("Number or name expected after -s");
                            return 1;
                        }

                        try
                        {
                            source = getEncoding(args[i]);
                        }
                        catch (ArgumentException)
                        {
                            Console.Error.WriteLine("Unknown encoding : " + args[i]);
                            return 2;
                        }

                        break;

                    case "-d":
                        //Encodage destination
                        if (dest != null)
                        {
                            Console.Error.WriteLine("-d is already specified");
                            return 1;
                        }

                        i++;
                        if (i == args.Length)
                        {
                            Console.Error.WriteLine("Number or name expected after -d");
                            return 1;
                        }

                        try
                        {
                            dest = getEncoding(args[i]);
                        }
                        catch (ArgumentException)
                        {
                            Console.Error.WriteLine("Unknown encoding : " + args[i]);
                            return 2;
                        }

                        break;

                    case "-r":
                        //Répertoire destination
                        if (repertoireDest != null)
                        {
                            Console.Error.WriteLine("-r is already specified");
                            return 1;
                        }

                        i++;
                        if (i == args.Length)
                        {
                            Console.Error.WriteLine("Directory path expected after -d");
                            return 1;
                        }

                        repertoireDest = validerRepertoireDest(args[i]);

                        break;

                    default:
                        chemins.Add(validerFichier(args[i]));
                        break;
                }
            }

            if (chemins.Count == 0)
            {
                Console.Error.WriteLine("No file to convert");
                return 1;
            }

            if (source == null) source = Encoding.UTF8;
            if (dest == null) dest = Encoding.UTF8;

            foreach (string chemin in chemins) {
                string contenu = File.ReadAllText(chemin, source);
                string cheminDest = chemin;
                if (repertoireDest != null)
                {
                    cheminDest = Path.Combine(repertoireDest, Path.GetFileName(chemin));
                }
                File.WriteAllText(cheminDest, contenu, dest);
            }

            return 0;
        }

        static Encoding getEncoding(string nomOuNumero)
        {
            if (nomOuNumero == null)
            {
                return null;
            }
            else if (Regex.IsMatch(nomOuNumero, "^[0-9]+$"))
            {
                return Encoding.GetEncoding(int.Parse(nomOuNumero));
            }
            else
            {
                return Encoding.GetEncoding(nomOuNumero);
            }
        }

        static string validerRepertoireDest(string chemin)
        {
            chemin = Path.GetFullPath(chemin);
            Directory.CreateDirectory(chemin);
            return chemin;
        }

        static string validerFichier(string chemin)
        {
            chemin = Path.GetFullPath(chemin);
            if (!File.Exists(chemin))
            {
                throw new FileNotFoundException("File does not exists : " + chemin);
            }
            return chemin;
        }

        static void info()
        {
            Console.Out.WriteLine("Usage : encoder [options]... files");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Options :");
            Console.Out.WriteLine(" -s [number or name] : specifies source encoding, default is UTF-8");
            Console.Out.WriteLine(" -d [number or name] : specifies source encoding, default is UTF-8");
            Console.Out.WriteLine(" -r [directory path] : directory to copy files to, if unspecified source files are simply replaced");
            Console.Out.WriteLine(" -e : lists available encodings");
            
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Examples :");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Converts two files from UTF-7 to UTF-8 :");
            Console.Out.WriteLine(" encoder -s UTF-7 file1.txt file2.txt");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Converts a file from UTF-7 to UTF-8 placing new file in C:\\Temp:");
            Console.Out.WriteLine(" encoder -s UTF-7 -r C:\\Temp file.txt");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Converts a file from ANSI Latin 1 (french) to UTF-7 :");
            Console.Out.WriteLine(" encoder -s 1252 -d UTF-7 file.txt");
        }
            
        static void listerEncodages()
        {
            Console.Out.WriteLine("List of available encodings [number] [name] [label]");
            Console.Out.WriteLine(String.Join(Environment.NewLine, nomsEncodages().ToArray()));
        }

        static IList<string> nomsEncodages()
        {
            List<string> liste = new List<string>();
            foreach (EncodingInfo e in Encoding.GetEncodings())
            {
                liste.Add(String.Concat(e.CodePage, " - ", e.Name, " - ", e.DisplayName));
            }
            //liste.Sort();
            return liste;
        }
    }
}
