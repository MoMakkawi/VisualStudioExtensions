using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace FCM
{
    public class Dgml
    { 

        public static List<MyFunction> Functions = new List<MyFunction>();
        public static List<Flow> Flows = new List<Flow>();
        static readonly List<MyCategory> Categories = new List<MyCategory>()
        {
            new MyCategory("Par","Green"),
            new MyCategory("Alt","Yellow"),
            new MyCategory("Cho","Blue"),
            new MyCategory("Ite","Red")
        };

        public static byte[] GeneratDGMLFile()
        {
            //CosmeticImprovements
            Functions.ForEach(f => f.Lable = f.Lable.Trim());

            DgmlBuilder builder = new DgmlBuilder
            {
                //convert Types ( I wrote it ) to Orginal Types ( OpenSoftware.DgmlTools.Model ) 

                NodeBuilders = new List<NodeBuilder> { new NodeBuilder<MyFunction>(Function2Node) },
                LinkBuilders = new List<LinkBuilder> { new LinkBuilder<Flow>(Flow2Link) },
                CategoryBuilders = new List<CategoryBuilder> { new CategoryBuilder<MyCategory>(CreatCategory) }
            };

            DirectedGraph DG = new DirectedGraph();

            DG = builder.Build(Functions, Flows, Categories);

            DG.GraphDirection = GraphDirection.LeftToRight; 
            DG.Layout = Layout.Sugiyama;

            return ConvertDirectedGraphToByteArray(DG);
        }
        static byte[] ConvertDirectedGraphToByteArray(DirectedGraph graph)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DirectedGraph));
            MemoryStream memoryStream = new MemoryStream();

            // Serialize DirectedGraph graph to  DGML Code
            // in memoryStream (to put DGML Code in memory as stream instead of a disk or a network connection)
            // as UTF8 (VS need DGML code as byte array in utf8)

            var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            serializer.Serialize(streamWriter, graph);

            byte[] utf8EncodedXml = memoryStream.ToArray();

            return utf8EncodedXml;
        }

        static Node Function2Node(MyFunction fun) => new Node { Id = fun.Id, Label = fun.Lable, Description = fun.Info, Group = fun.Group, Category = fun.Category };
        static Link Flow2Link(Flow call) => new Link { Source = call.Source, Target = call.Target, Category = call.Category };
        static Category CreatCategory(MyCategory category) => new Category { Id = category.Id, Background = category.Background };

    }
}
