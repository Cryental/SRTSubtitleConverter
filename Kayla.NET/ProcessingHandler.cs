﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kayla.NET.Converters;
using Kayla.NET.Parsers;

namespace Kayla.NET
{
    public class ProcessingHandler
    {
        private readonly Dictionary<string, ISubtitleConverter> _supportedConverters =
            new Dictionary<string, ISubtitleConverter>();

        private readonly Dictionary<string, ISubtitleParser> _supportedParsers =
            new Dictionary<string, ISubtitleParser>();

        public ProcessingHandler()
        {
            _supportedParsers.Add("MicroDVD", new MicroDVDParser());
            _supportedParsers.Add("SAMI", new SAMIParser());
            _supportedParsers.Add("SubStationAlpha", new SSAParser());
            _supportedParsers.Add("SubViewer", new SubViewerParser());
            _supportedParsers.Add("TimedText", new TTMLParser());
            _supportedParsers.Add("WebVTT", new VTTParser());
            _supportedParsers.Add("YtXml", new YtXmlParser());
            _supportedParsers.Add("SubRip", new SRTParser());

            _supportedConverters.Add("MicroDVD", new MicroDVDConverter());
            _supportedConverters.Add("SAMI", new SAMIConverter());
            _supportedConverters.Add("SubStationAlpha", new SSAConverter());
            _supportedConverters.Add("SubViewer", new SubViewerConverter());
            _supportedConverters.Add("SubRip", new SRTConverter());
        }

        public bool Convert(string inputPath, string outputPath, string format = "SubRip")
        {
            if (format == string.Empty)
            {
                format = "SubRip";
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("[!] The input file does not exist.");
                return false;
            }



            ISubtitleConverter selectedConverter = null;

            foreach (var converter in _supportedConverters.Where(converter => converter.Key == format))
            {
                selectedConverter = converter.Value;
                break;
            }

            if (selectedConverter == null)
            {
                Console.WriteLine("[!] The selected format is not supported.");
                return false;
            }

            if (Directory.Exists(outputPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(inputPath) + selectedConverter.FileExtension;
                outputPath = Path.Combine(outputPath, fileName);
            }

            var finalResult = string.Empty;

            foreach (var sf in _supportedParsers)
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

                        var result = selectedConverter.Convert(parsedData);

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

            File.WriteAllText(outputPath, finalResult, Encoding.UTF8);
            Console.WriteLine($"[+] Converted File: {Path.GetFileName(outputPath)}");
            Console.WriteLine("[*] The operation is completed.");
            return true;
        }

        public bool ConvertBath(string inputPath, string outputPath, string format = "SubRip")
        {
            if (format == string.Empty)
            {
                format = "SubRip";
            }

            if (!Directory.Exists(inputPath))
            {
                Console.WriteLine("[!] The input path is not a directory or does not exist.");
                return false;
            }

            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("[!] The output path is not a directory or does not exist.");
                return false;
            }

            ISubtitleConverter selectedConverter = null;

            foreach (var converter in _supportedConverters.Where(converter => converter.Key == format))
            {
                selectedConverter = converter.Value;
                break;
            }

            if (selectedConverter == null)
            {
                Console.WriteLine("[!] The selected format is not supported.");
                return false;
            }

            var files = new DirectoryInfo(inputPath);

            var convertedFiles = new List<string>();
            var unconvertedFiles = new List<string>();

            foreach (var f in files.GetFiles())
            {
                var outputFilePath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(f.Name) + selectedConverter.FileExtension);
                var finalResult = string.Empty;

                foreach (var sf in _supportedParsers)
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