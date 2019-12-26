using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tetris
{
    public class FieldBlock
    {
        [JsonProperty(PropertyName = "isEmpty")]
        public bool isEmpty;
        [JsonProperty(PropertyName = "Color")]
        public string Color;
    }

    public class Field
    {
        [JsonProperty(PropertyName = "block")]
        public List<List<FieldBlock>> board = new List<List<FieldBlock>>();

        [JsonProperty(PropertyName = "score")]
        public int score;

        [JsonProperty(PropertyName = "time")]
        public string date;

        [JsonProperty(PropertyName = "uuid")]
        public string uuid;
    }
    
    public class StackInfo
    {
        [JsonProperty(PropertyName = "uuid")]
        public string stack_owner;
        [JsonProperty(PropertyName = "stack")]
        public int stack_size;
    }
}
