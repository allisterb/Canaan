namespace Canaan

open System
open System.IO
open System.Threading.Tasks
open System.Threading

type IO() =
    inherit Api()

    override x.Initialized = true

    static member GetFiles recurse pattern srcPath =
        if not <| Directory.Exists srcPath then
            err "The path {0} does not exist." [srcPath]
            (-1, Array.empty)
        else
            let files = Directory.GetFiles(srcPath, pattern, recurse ? (SearchOption.AllDirectories, SearchOption.TopDirectoryOnly))
            
            (files.Length, files)
    
    static member GetAllFiles pattern srcPath = IO.GetFiles true pattern srcPath

    static member GetDirFiles pattern srcPath = IO.GetFiles false pattern srcPath
    
    static member GetFilesAsync recurse srcPath pattern =
        async { return IO.GetFiles recurse srcPath pattern }

    static member DeleteFiles recurse srcPath pattern =
        if not <| Directory.Exists srcPath then
            do Api.Error("The path {0} does not exist.", srcPath)
            -1
        else
            let count, files = IO.GetFiles recurse pattern srcPath
            for file in files do
                File.Delete <| Path.Combine(srcPath, file)
            files.Length

    
