#load "SerializableDictionary.cs"
#load "VehicleStructsBmw.cs"
using System.IO;
using System.Xml;

Output.BuildAction = BuildAction.Content;

string xmlFile = string.Empty;
string sqlUser = "user";
string sqlPassword = "password";
string accessPassword = string.Empty;
string testLic = string.Empty;

try
{
    string fileName = Path.ChangeExtension(Context.ScriptFilePath, ".xml");
    if (File.Exists(fileName))
    {
        xmlFile = fileName;
    }
}
catch (Exception ex)
{
    Output.WriteLine("Exception: {0}", ex.Message);
    return;
}

Output.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<info>
    <add key=""IstaFolder"" value=""{xmlFile}""/>
</info>");
