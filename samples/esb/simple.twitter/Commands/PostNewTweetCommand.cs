using Carbon.Core.Stereotypes.For.Components.Message;

namespace Commands
{
    /// <summary>
    /// Command sent to the domain for indicating that a
    /// new "tweet" has been posted by a user.
    /// </summary>
    [Message]
    public class PostNewTweetCommand
    {
        public string Who { get; set; }
        public string Message { get; set; }
    }
}