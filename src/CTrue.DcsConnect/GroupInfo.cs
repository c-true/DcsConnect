namespace CTrue.DcsConnect
{
    public class GroupInfo
    {
        public uint GroupId { get; }
        public string Name { get; set; }

        public uint Coalition { get; set; }
        public uint Category { get; set; }

        public GroupInfo(uint groupId)
        {
            GroupId = groupId;
        }
    }
}