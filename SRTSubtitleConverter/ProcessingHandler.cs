﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SRTSubtitleConverter.Converters;
using SRTSubtitleConverter.Parsers;

namespace SRTSubtitleConverter
{
    public class ProcessingHandler
    {
        private readonly Dictionary<string, ISubtitleParser> _supportedFormats =
            new Dictionary<string, ISubtitleParser>();

        public ProcessingHandler()
        {
            _supportedFormats.Add("MicroDVD", new MicroDVDParser());
            _supportedFormats.Add("SAMI", new SAMIParser());
            _supportedFormats.Add("SubStation Alpha", new SSAParser());
            _supportedFormats.Add("SubViewer 2.0", new SubViewerParser());
            _supportedFormats.Add("Timed Text", new TTMLParser());
            _supportedFormats.Add("WebVTT", new VTTParser());
            _supportedFormats.Add("Youtube Subtitle XML", new YtXmlParser());
            _supportedFormats.Add("SubRip", new SRTParser());
        }

        public bool ConvertToSRT(string inputPath, string outputPath, bool folderFlag = false)
        {
            var finalResult = string.Empty;

            var outputFilePath = outputPath;

            if (folderFlag)
            {
                outputFilePath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputPath) + ".srt");
            }

            foreach (var sf in _supportedFormats)
            {
                var extensions = sf.Value.FileExtension.Split('|');

                foreach (var ext in extensions)
                {
                    if (Path.GetExtension(inputPath) == ext)
                    {
                        var parsingStatus = sf.Value.ParseFormat(inputPath, out var parsedData);

                        if (!parsingStatus)
                        {
                            continue;
                        }

                        var srtConverter = new SRTConverter();
                        var result = srtConverter.Convert(parsedData);

                        if (!string.IsNullOrEmpty(result))
                        {
                            finalResult = result;
                            break;
                        }
                    }
                }
            }


            if (string.IsNullOrEmpty(finalResult))
            {
                return false;
            }

            File.WriteAllText(outputFilePath, finalResult, Encoding.UTF8);
            Console.WriteLine($"[+] Converted File: {Path.GetFileName(inputPath)}");
            Console.WriteLine("[*] The operation is completed.");
            return true;
        }

        public bool ConvertBathToSRT(string inputPath, string outputPath)
        {
            var files = new DirectoryInfo(inputPath);

            var convertedFiles = new List<string>();
            var unconvertedFiles = new List<string>();

            foreach (var f in files.GetFiles())
            {
                var outputFilePath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(f.Name) + ".srt");
                var finalResult = string.Empty;

                foreach (var sf in _supportedFormats)
                {
                    var extensions = sf.Value.FileExtension.Split('|');

                    foreach (var ext in extensions)
                    {
                        if (Path.GetExtension(f.Name) != ext)
                        {
                            continue;
                        }

                        var parsingStatus = sf.Value.ParseFormat(inputPath, out var parsedData);

                        if (!parsingStatus)
                        {
                            continue;
                        }

                        var srtConverter = new SRTConverter();
                        var result = srtConverter.Convert(parsedData);

                        if (string.IsNullOrEmpty(result))
                        {
                            continue;
                        }

                        finalResult = result;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(finalResult))
                {
                    File.WriteAllText(outputFilePath, finalResult, Encoding.UTF8);
                    convertedFiles.Add(f.Name);
                }
                else
                {
                    unconvertedFiles.Add(f.Name);
                }
            }

            Console.WriteLine("[+] Converted Files ---");
            Console.WriteLine();
            foreach (var f in convertedFiles)
            {
                Console.WriteLine($"[-] {f}");
            }

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("[+] Not Converted Files ---");
            Console.WriteLine();
            foreach (var f in unconvertedFiles)
            {
                Console.WriteLine($"[-] {f}");
            }

            Console.WriteLine();

            Console.WriteLine("[*] The operation is completed.");
            return true;
        }
    }
}