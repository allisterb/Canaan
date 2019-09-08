using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;
using CommandLine.Text;

namespace NewsAlpha.CLI
{
    class Options
    {
        [Option("verbose", HelpText = "Enable verbose logging.", Default = false)]
        public bool Verbose { get; set; }

    }

    [Verb("social", HelpText  = "Search social news posts")]
    class SocialOptions : Options
    {
        [Option('s', "search", Required = true, HelpText = "Search social media posts for the specified text")]
        public string Search { get; set; }

        [Option('i', "hate-speech", Required = true, HelpText = "The Azure Storage resource or local directory that will be the sync destination. Use a single directory name if specifying a local sync destination. For an Azure Storage resource you must use your Blob Service endpoint Uri.")]
        public string Destination { get; set; } 

        [Option('b', "begin", HelpText = "The date and ", Default = "*")]
        public string End { get; set; }

        [Option('S', "recurse", HelpText = "Recurse into lower level sub-directories when searching for local file or directory names that match a pattern.", Default = false)]
        public bool Recurse { get; set; }

        [Option('r', "retry-count", HelpText = "The number of times to retry an Azure Storage operation which does not complete successfully.", Default = 3)]
        public int RetryCount { get; set; }

        [Option("retry-wait", HelpText = "The number of seconds to wait between retries.", Default = 10)]
        public int RetryWait { get; set; }

        [Option("source-key", HelpText = "The account key for accessing the source Azure Storage resource.")]
        public string SourceKey { get; set; }

        [Option("dest-key", HelpText = "The account key for accessing the destination Azure Storage resource.")]
        public string DestinationKey { get; set; }

        [Option("overwrite", HelpText = "Overwrite existing destination Azure Storage objects or local files or directories during a copy operation.", Default = false)]
        public bool Overwrite { get; protected set; }

        [Option("use-emulator", HelpText = "Use the Azure Storage emulator installed on the local machine.", Default = false)]
        public bool UseStorageEmulator { get; set; }

        [Option("block-size", HelpText = "The file block size in kilobytes. Default is 4096 (4MB). For Azure Storage block blobs this is also the blob block size.", Default = 4096)]
        public int BlockSizeKB { get; set; }

        [Option("content-type", HelpText = "The content-type to set destination Azure Storage blobs to.")]
        public string ContentType { get; set; }

        [Option('j', "journal-file", HelpText = "Full path and name of the local transfer journal file to use during file transfers. This file is used to resume uploads or downloads that were interrupted. If you do not specify this then a default name will be used.")]
        public string JournalFilePath { get; set; }

        [Option("no-journal", HelpText = "Do not use a local transfer journal during transfer. If a previous journal file for the transfer exists then it will be deleted.")]
        public bool NoJournal { get; set; }

        [Option("delete-journal", HelpText = "Delete any existing local transfer journal file before transfer. A new journal file for the transfer will be created unless the --no-journal option is also specified.")]
        public bool DeleteJournal { get; set; }
    }


    [Verb("gen", HelpText = "Generate a file for testing AzSync with your Azure Storage account.")]
    class GenerateOptions : Options
    {
        [Option("name", Required = true, HelpText = "The name of the test file to generate or modify.")]
        public string Name { get; set; }

        [Option("size", Required = true, HelpText = "Set the size in MB of the test file to the specified value. If the file exists then it will be modified.")]
        public int SizeMB { get; set; }

        [Option("part-size", Required = false, Default = 100, HelpText = "Set the average size in KB of each part of the test file. Default is 100.")]
        public int PartSizeKB { get; set; }
    }
}
