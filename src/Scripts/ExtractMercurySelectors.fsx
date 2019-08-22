open System
open System.Collections.Generic
open System.IO;
open System.Linq;
open System.Text.RegularExpressions;

let parserRegex = Regex("export\s*const\s*(\w+)\s*=\s*(\{[\s\S]+\});", RegexOptions.Compiled)
let commentsRegex = Regex("^\s*\/\/.*$", RegexOptions.Compiled ||| RegexOptions.Multiline)
let domainRegex = Regex("domain:\s*('\S+'),", RegexOptions.Compiled)
let supportedDomainsRegex = Regex("supportedDomains:\s*\[[\s\S]+?\],", RegexOptions.Compiled)
let contentSelectorsRegex = Regex("(content:\s*\{\s*selectors:\s*\[[\S\s]+\]),[\s\S]*transforms", RegexOptions.Compiled)
let extractorsDir = Directory.EnumerateDirectories(@"C:\Projects\mercury-parser\src\extractors\custom")
let output = new StreamWriter(File.Open("C:\Projects\Canaan\src\Scripts\mercury-content-selectors.json", FileMode.Create))
output.Write("{" + Environment.NewLine)
for dir in extractorsDir do
    let f = Directory.EnumerateFiles(dir, "index.js", SearchOption.TopDirectoryOnly).First();
    let text = commentsRegex.Replace(File.ReadAllText(f), "")
    let m = parserRegex.Match(text)
    do if m.Success then 
            let name = m.Groups.Item(1).Value
            let selectors = m.Groups.Item(2).Value
            let m2 = contentSelectorsRegex.Match(selectors)
            let m3 = domainRegex.Match(text)
            let m4 = supportedDomainsRegex.Match(text)
            let content = if m2.Success then m2.Groups.Item(1).Value else ""
            let domain = if m3.Success then m3.Groups.Item(0).Value else ""
            let supportedDomains = if m4.Success then m4.Groups.Item(0).Value else "supportedDomains: [" + m3.Groups.Item(1).Value + "],\n"  
            if content <> "" && domain <> "" then output.Write("\t" + name + ":\n\t{\n\t\t" + domain + "\n\t\t" + supportedDomains + "\n\t\t" + content +  "\t\t\n}" + "\n},\n") else printfn "Could not determine content selectors for %s." name
       else printfn "Did not match expression in dir %s" dir
output.Write("}" + Environment.NewLine)
output.Close()
printfn "Done"
