namespace MySpace.Models
{
    public class FriendShipState
    {
        public User TargetUser { get; set; }
        public int Status { get; set; }

        public FriendShipState (User user, int status)
        {
            TargetUser = user;
            Status = status;
        }
    }
}