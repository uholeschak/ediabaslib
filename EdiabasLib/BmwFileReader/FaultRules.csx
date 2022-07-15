#load "SerializableDictionary.cs"
#load "VehicleStructsBmw.cs"
using System.IO;
using System.Xml;
using System.Xml.Serialization;

Output.BuildAction = BuildAction.Content;

string xmlFile = string.Empty;
VehicleStructsBmw.FaultRulesInfoData faultRulesInfoData = null;

try
{
    string fileName = Path.ChangeExtension(Context.ScriptFilePath, ".xml");
    if (File.Exists(fileName))
    {
        xmlFile = fileName;
        using (StreamReader sr = new StreamReader(fileName))
        {
            Type faultRulesType = typeof(VehicleStructsBmw.FaultRulesInfoData);
            XmlSerializer serializer = new XmlSerializer(faultRulesType);
        //    faultRulesInfoData = serializer.Deserialize(sr) as VehicleStructsBmw.FaultRulesInfoData;
        }
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
