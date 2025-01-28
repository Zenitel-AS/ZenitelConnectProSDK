using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    public class PrerecordedMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dirno { get; set; } 
        [NotMapped]
        public string GroupNo { get; set; }
    }
}
