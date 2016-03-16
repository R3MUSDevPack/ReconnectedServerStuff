using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DotlanMapTranslator
{
    class Program
    {
        private static XDocument xDoc;

        static void Main(string[] args)
        {
            var dInfo = new DirectoryInfo(@"C:\temp\Maps\To Process");
            var fInfos = dInfo.EnumerateFiles().ToList();

            fInfos.ForEach(fInfo => {
                LoadMap(fInfo.FullName);
                if (xDoc != null)
                {
                    EditXDoc();
                    SaveXDoc(fInfo.Name);
                    fInfo.MoveTo(string.Format(@"C:\temp\Maps\Archive\{0}", fInfo.Name));
                }
            });            
        }

        private static void LoadMap(string mapName)
        {
            try
            {
                xDoc = new XDocument();
                xDoc = XDocument.Load(mapName);
                xDoc.Root.AddBeforeSelf(new XProcessingInstruction("xml-stylesheet", "type='text/css' href='../Content/MapStyle.css'"));
            }
            catch(Exception e)
            {
            }
        }

        private static void EditXDoc()
        {
            var svg = xDoc.Elements().LastOrDefault();
            svg.Attribute("style").Value = "stroke-width: 0px; background: #111111;";

            svg.Elements().FirstOrDefault().Remove();

            var mapNode = svg.Elements().Where(node => node.Name.LocalName == "g").FirstOrDefault();
            mapNode.FirstNode.Remove();

            var symbNodes = mapNode.Elements().FirstOrDefault().Elements().Where(sNode => 
            (sNode.Name.LocalName == "symbol") 
            && (sNode.Elements().FirstOrDefault().Name.LocalName == "a"));

            symbNodes.ToList().ForEach(symNode =>
            {
                var txt = symNode.Elements().FirstOrDefault().Elements().LastOrDefault();
                if((txt.FirstAttribute.Name.LocalName == "id") && (txt.FirstAttribute.Value.Contains("txt")))
                {
                    txt.Remove();
                }
            });            
        }

        private static void SaveXDoc(string mapName)
        {
            xDoc.Save(string.Format(@"C:\temp\Maps\Processed\{0}", mapName));
        }
    }
}
