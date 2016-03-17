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
        public enum Map
        {
            Aridia,
            Black_Rise,
            Branch,
            Cache,
            Catch,
            Cloud_Ring,
            Cobalt_Edge,
            Curse,
            Deklein,
            Delve,
            Derelik,
            Detorid,
            Devoid,
            Domain,
            Esoteria,
            Essence,
            Etherium_Reach,
            Everyshore,
            Fade,
            Feythabolis,
            Fountain,
            Geminate,
            Genesis,
            Great_Wildlands,
            Heimatar,
            Immensea,
            Impass,
            Insmother,
            Kador,
            Khanid,
            Lonetrek,
            Malpais,
            Metropolis,
            Molden_Heath,
            Oasa,
            Omist,
            Outer_Passage,
            Outer_Ring,
            Paragon_Soul,
            Period_Basis,
            Perrigen_Falls,
            Placid,
            Providence,
            Pure_Blind,
            Querious,
            Scalding_Pass,
            Sinq_Laison,
            Solitude,
            Stain,
            Syndicate,
            Tenal,
            Tenerifis,
            The_Bleak_Lands,
            The_Citadel,
            The_Forge,
            The_Kalevala_Expanse,
            The_Spire,
            Tribute,
            Vale_of_the_Silent,
            Venal,
            Verge_Vendor,
            Wicked_Creek
        }

        private static string dotlanURI = @"http://evemaps.dotlan.net/svg/{0}.svg";

        static void Main(string[] args)
        {
            //var dInfo = new DirectoryInfo(@"C:\temp\Maps\To Process");
            //var fInfos = dInfo.EnumerateFiles().ToList();

            //fInfos.ForEach(fInfo => {
            //    LoadMap(fInfo.FullName);
            //    if (xDoc != null)
            //    {
            //        EditXDoc();
            //        SaveXDoc(fInfo.Name);
            //        fInfo.MoveTo(string.Format(@"C:\temp\Maps\Archive\{0}", fInfo.Name));
            //    }
            //});
            foreach(var mapName in Enum.GetValues(typeof(Map)))
            {
                try
                {
                    LoadMap(string.Format(dotlanURI, mapName.ToString()));
                    if (xDoc != null)
                    {
                        Console.WriteLine("Editing {0}", mapName.ToString());
                        EditXDoc();
                        Console.WriteLine("Saving {0}", mapName.ToString());
                        SaveXDoc((Map)mapName);
                        Console.WriteLine("Successfully imported {0}", mapName.ToString());
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(string.Format("Error on map {0}: {1}", mapName.ToString(), ex.Message));
                }
            }
            Console.ReadLine();
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
                Console.WriteLine(string.Format("Error encountered during import: {0}", e.Message));
                xDoc = null;
            }
        }

        private static void EditXDoc()
        {
            var changeClass = false;
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
                var editElements = symNode.Elements().FirstOrDefault().Elements();

                var colourRect = editElements.Where(elem => elem.Attribute("id").Value.Contains("rect")).FirstOrDefault();
                colourRect.Attributes().ToList().ForEach(attr =>
                {
                    if(attr.Name.LocalName == "style")
                    {
                        attr.Remove();
                        changeClass = true;
                    }
                });

                if(changeClass)
                {
                    colourRect.SetAttributeValue("class", "s");
                    changeClass = false;
                }

                var txt = symNode.Elements().FirstOrDefault().Elements().LastOrDefault();
                if ((txt.FirstAttribute.Name.LocalName == "id") && (txt.FirstAttribute.Value.Contains("txt")))
                {
                    txt.Remove();
                }
            });            
        }

        private static void SaveXDoc(string mapName)
        {
            xDoc.Save(string.Format(@"C:\temp\Maps\Processed\{0}", mapName));
        }
        private static void SaveXDoc(Map mapName)
        {
            if(!Directory.Exists(@"C:\Maps\"))
            {
                Directory.CreateDirectory(@"C:\Maps\");
            }
            xDoc.Save(string.Format(@"C:\Maps\{0}.svg", mapName.ToString()));
        }
    }
}
