using FFMpegCore;
using FFMpegCore.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stream_Video_Merger__Core_.Settings;

namespace Stream_Video_Merger__Core_
{
    class Program
    {
        //This just takes in stream recordings and kicks out one big video
        static void Main(string[] args)
        {
            try
            {
                List<string> allVideos = new List<string>();
                List<string> allMusic = new List<string>();
                Settings.LoadSettings("StreamVideoMerger");

                if (args.Length == 0)
                {
                    Console.WriteLine("No input files; drag all stream files into the exe to merge into one long recording. Videos will be merged in alphabetically");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Enter result filename [result will be x.mp4]: ");
                string resultName = Console.ReadLine();

                String resultVideo = resultName + "." + Settings.ResultFileExtension;

                string[] audioFileTypes = { ".MP3", ".WAV", ".OGG", ".WMA" };    //Check if ffmpeg can even use crap like .wma
                                                                                 //Assuming this shit will always come in alp
                foreach (var item in args)
                {
                    var mediaInfoo = FFProbe.Analyse(item);
                    if (!(audioFileTypes.Contains(Path.GetExtension(item).ToUpper()))) //Is there a non-shit way to do this?
                        allVideos.Add(item);
                    else
                        allMusic.Add(item);
                }

                allVideos.Sort();   //Alphabetically because why would you want a later part of the piece in the front?

                Console.WriteLine("Merging:");
                foreach (var item in allVideos)
                {
                    Console.WriteLine(item.ToString());
                }

                try
                {
                    FFMpeg.Join(resultVideo,
                       allVideos.ToArray()
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error encountered when attempting to merge. This is most likely because you don't have FFMpeg.exe and FFProbe.exe in your PATH");
                    Console.WriteLine("Actual error trace: " + e.Message);
                    Console.ReadKey();
                    return;
                }

                FileInfo f = new FileInfo(resultVideo);
                string fullname = f.FullName;

                Console.WriteLine("Result: " + fullname);
                if (Settings.MakeZip)
                {
                    Console.WriteLine("Making zip version");
                    makeZip(resultName, fullname);
                    Console.WriteLine("Zip version created");
                }

                if (Settings.MakeWebm)
                {
                    Console.WriteLine("Generating webm version (NOTE: this will take a while for large files!)");
                    FFMpegArguments
                    .FromFileInput(resultVideo)
                    .OutputToFile(resultVideo.Replace("mp4", "webm"), true)
                    .ProcessSynchronously();

                }
                Console.WriteLine("Done");
                Console.ReadKey();
                return;
            }catch(Exception e)
            {
                Console.WriteLine("Misc error encountered");
                Console.WriteLine("Actual error trace: " + e.Message);
                Console.ReadKey();
                return;
            }
        }
        static string makeZip(string filename, string mergedFileLocation)
        {
            var archiveFilePath = filename + @".zip";
            var archive = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(mergedFileLocation, Path.GetFileName(mergedFileLocation));
            archive.Dispose();
            return archiveFilePath;
        }
    }
}
