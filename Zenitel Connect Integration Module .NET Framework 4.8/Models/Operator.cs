using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    public class Operator
    {
        [Key]
        public string MachineName { get; set; }
        [NotMapped]
        public string DirectoryNumber { get; set; }

        private Configuration _Configuration;
        public Configuration Confguration
        {
            get
            {
                if (_Configuration == null)
                    _Configuration = new Configuration();
                return _Configuration;
            }
            set { _Configuration = value; }
        }

        public Operator()
        {
            this.Confguration.FillEmptyValuesWithDefaults();
        }
    }
}
