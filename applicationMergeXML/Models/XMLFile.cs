using System.ComponentModel.DataAnnotations;

namespace applicationMergeXML.Models
{
    public class XMLFile
    {
        [Key]
        public int fileid { get; set; }
        public string name { get; set; }

    }
}
