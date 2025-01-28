using ConnectPro.Models;
using Wamp.Client;

namespace ConnectPro.Handlers
{
    public class AccessControlHandler
    {
        Events _events;
        WampClient _wamp;

        public AccessControlHandler(Events events, WampClient wamp)
        {
            _events = events;
            _wamp = wamp;

            _wamp.OnWampOpenDoorEvent += HandleWampOpenDoorEvent;
        }

        private void HandleWampOpenDoorEvent(object sender, WampClient.wamp_open_door_event doorOpenEvent)
        {
        }
        public void OpenDoor(Device ele)
        {
            _wamp.PostOpenDoor(ele.dirno);
        }
    }
}
