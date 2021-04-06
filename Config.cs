using Torch;

namespace ShittyFactionPlugin
{
    public class Config : ViewModel
    {
        private bool _enable;
        private bool _assignNewPlayers;
        private int _factionMemberLimit;
        private string _joinDenyMsg = "Faction join request denied";
        private bool _keepDefaultEnemy;


        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                OnPropertyChanged();
            }
        }


        public bool AssignFaction
        {
            get => _assignNewPlayers;
            set
            {
                _assignNewPlayers = value;
                OnPropertyChanged();
            }
        }

        public int FactionSize
        {
            get => _factionMemberLimit;
            set
            {
                _factionMemberLimit = value;
                OnPropertyChanged();
            }
        }

        public string JoinDenyMessage
        {
            get => _joinDenyMsg;
            set
            {
                _joinDenyMsg = value;
                OnPropertyChanged();
            }
        }

        public bool KeepDefaultEnemy
        {
            get => _keepDefaultEnemy;
            set
            {
                _keepDefaultEnemy = value;
                OnPropertyChanged();
            }
        }
    }
}