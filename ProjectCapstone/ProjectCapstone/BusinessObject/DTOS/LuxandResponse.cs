using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class Person
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public List<Collection> Collections { get; set; }
    }

    public class Collection
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
    }

    public class Face
    {
        public string Uuid { get; set; }
        public string Url { get; set; }
    }

    public class LuxandResponse
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public List<Face> Faces { get; set; }
        public List<Collection> Collections { get; set; }
    }

}
