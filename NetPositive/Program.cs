﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using NetPositive.Scanner;
using NetPositive.Scanner.Results;

namespace NetPositive
{
    class Program
    {
        static void Main(string[] args)
        {
            ScanArguments scanArguments = ArgumentParser.ParseArguments(args);
            if (scanArguments.printHelp)
                return;
            FileIterator scanTargets = new FileIterator(scanArguments.basePaths.ToArray(), scanArguments.recursive, scanArguments.allowPathEscape);
            
            //Initialize Scanning Modules from CLI Spec
            IEnumerable<IScanner> scanners = ScannerHelper.GetScannersFromSpec(scanArguments.scanSpec);

            if(scanners.Count()==0)
            {
                Console.WriteLine("Error: No scanners specified, aborting. Please specify with -S");
                ArgumentParser.PrintHelp();
                return;
            }

            IResultContainer results;
            if (scanArguments.aggregatorUri != null)
            {
                results = new AggregatorResultContainer(scanArguments.aggregatorUri, args);
            }
            else
            {
                if (scanArguments.outputPath != null)
                {
                    results = new FileResultContainer(scanArguments.outputPath);
                }
                else
                {
                    results = new CLIResultContainer();
                }
            }
            //Iterate over all targets and run scanners
            foreach (string path in scanTargets)
            {
                foreach(IScanner scanner in scanners)
                {
                    scanner.Scan(results, path);
                }
            }
        }
    }
}
