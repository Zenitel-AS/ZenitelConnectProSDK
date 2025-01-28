using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectPro.Models
{
    public class Group : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Name { get; set; }
        public int OrderNo { get; set; }
        public string GroupNo { get; set; }
        //
        private bool _isBussy;
        [NotMapped]
        public bool IsBussy
        {
            get { return _isBussy; }
            set
            {
                if (_isBussy != value)
                {
                    _isBussy = value;
                    OnPropertyChanged("IsBussy");

                    if (_isBussy == false)
                    {
                        BrodcastedMessageName = "Prerecorded Msg";
                    }
                }
            }
        }
       
        private string _broadcastedMessageName = "Prerecorded Msg";
        [NotMapped]
        public string BrodcastedMessageName
        {
            get { return _broadcastedMessageName; }
            set
            {
                if(_broadcastedMessageName != value)
                {
                    _broadcastedMessageName = value;
                    OnPropertyChanged("BrodcastedMessageName");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
