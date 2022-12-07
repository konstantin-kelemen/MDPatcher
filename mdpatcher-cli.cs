using System;
using System.IO;
using System.Linq;
using System.Threading;
using DiscUtils;
using DiscUtils.Common;
using DiscUtils.Iso9660;

class MDPatcher
{ 
    public static string versionString = "1.0";
    public static string copyrightString = "(c) 2017 Giuseppe Gatta / nextvolume";
    
    public static Pattern[] patterns = 
    {
        new Pattern1(), new Pattern2(), new Pattern3(), new Pattern4(), new Pattern5(), new Pattern6(), new Pattern7()
    };
        
    public static int Scan(CDReader cd, DiscDirectoryInfo d, RawImageStream fstream)
    {
        int numPatches = 0;

        foreach(DiscFileInfo fi in d.GetFiles())
        {
            using(Stream file = cd.OpenFile(fi.FullName, FileMode.Open))
            {                 
                // Scan file for patterns                 
                byte[] readBuffer = new byte[file.Length];
                file.Seek(0, SeekOrigin.Begin);

                long filePos;
                    
                file.Read(readBuffer, 0, 1);
                filePos = fstream.Seek(0, SeekOrigin.Current) - 1;
                file.Read(readBuffer, 1, (int)file.Length - 1); 

                foreach(Pattern p in patterns)
                {
                    byte[] patternBytes = p.GetPattern();
                    byte[] patchedPatternBytes = p.GetPatchedPattern();
                        
                    int length = patternBytes.Length;
                        
                    for(int i = 0; i < file.Length - length; i++)
                    {
                        int x;
                            
                        for(x = 0; x < length; x++)
                        {
                            if(readBuffer[i+x] != patternBytes[x])
                                break;
                        }
                            
                        if(x == length)
                        {
                            // Found the bytes for the pattern
                            Console.WriteLine(p.GetName() + " found at 0x" + i.ToString("X") + " in " + fi.FullName);
                            
                            // Patch!
                            try
                            {
                                long old = fstream.Seek(0, SeekOrigin.Begin);
                                
                                fstream.Seek(filePos+i, SeekOrigin.Begin);
                                fstream.Write(patchedPatternBytes, 0, length);
                                
                                fstream.Seek(old, SeekOrigin.Begin);
                                
                                numPatches++;
                            }
                            catch
                            {
                                // Error occurred, continue to next iteration
                                continue;
                            }
                        }
                        else
                        {
                            for(x = 0; x < length; x++)
                            {
                                if(readBuffer[i+x] != patchedPatternBytes[x])
                                    break;
                            }
                                
                            if(x == length)
                                // Found the bytes for the patched pattern
                                Console.WriteLine(p.GetName() + " (already patched) found at 0x" + i.ToString("X") + " in " + fi.FullName);
                        }
                    }
                }
            }
        }
            
        foreach (DiscDirectoryInfo di in d.GetDirectories())
            numPatches += Scan(cd, di, fstream);  

        return numPatches;
    }
		
	public static void Main(string[] args)
    {
        Console.WriteLine("MDPatcher " + versionString + " " + copyrightString + "\n");

        string filePath = "";
        CDReader cd = null;
        RawImageStream imageStream = null;

        if (args.Length < 1)
        {
            Console.WriteLine("Please enter the path to the file to patch: ");
            filePath = Console.ReadLine();
        }
        else
        {
            filePath = args[0];
        }

        if(!File.Exists(filePath))
        {
            Console.WriteLine("Error: File does not exist.");
            return;
        }
        else
        {
            int numPatches=0;

            using(imageStream = new RawImageStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                for(int x = 0; x < 3; x++)
                {
                    imageStream.Seek(0, SeekOrigin.Begin);
                    imageStream.SetMode(x);
                
                    try
                    {
                        using (cd = new CDReader(imageStream, true)) {
                            numPatches=Scan(cd, cd.Root, imageStream);
                        }
                        
                        break;
                    }
                
                    catch(Exception ex)
                    {
                        Console.WriteLine("Trying with MODE " + (x+1) + " ("+ex.Message+")");
                    }
                }
            }

    		Console.WriteLine(numPatches + " patch(es) applied");
        }
    }

}
