
namespace GameManager.Event
{
    internal sealed partial class EventPool
    {
        public sealed class Event
        {
            private object m_Sender;
            private GameEventArgs m_EventArgs;

            public Event()
            {
                m_Sender = null;
                m_EventArgs = null;
            }

            public object Sender
            {
                get { return m_Sender; }
            }

            public GameEventArgs EventArgs
            {
                get { return m_EventArgs; }
            }

            public static Event Create(object sender, GameEventArgs e)
            {
                Event eventNode = new Event();
                eventNode.m_Sender = sender;
                eventNode.m_EventArgs = e;
                return eventNode;
            }
        }
    }
    
}