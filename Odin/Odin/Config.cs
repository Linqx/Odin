using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin
{
    public class Config
    {
        [JsonProperty("port")]
        public int Port;

        public static Config FromResource(string resource)
        {
            using (StreamReader str = new StreamReader(resource))
            {
                string json = str.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}
