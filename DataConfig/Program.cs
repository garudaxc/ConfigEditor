using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DataConfig
{
    class Program
    {
        static void Main(string[] args)
        {

            DataContractSerializer serializer = new DataContractSerializer(typeof(Gift));
            FileStream stream = new FileStream(@"d:\test.xml", FileMode.Open);
            //serializer.Serialize(stream, value_);
            Gift value = (Gift)serializer.ReadObject(stream);

            stream.Close();

            value.WriteLine();

        }
    }
}
