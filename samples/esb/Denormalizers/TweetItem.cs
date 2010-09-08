namespace Denormalizers
{
    public class TweetItem
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            private set { Id = value;}
        }

        private string _message;
        public string Message
        {
            get {
                return _message;
            }
            set {
                _message = value;
            }
        }

        private string _who;
        public string Who
        {
            get { return _who; }
            set { _who = value; }
        }
    }
}