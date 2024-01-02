using System;

namespace CTrue.DcsConnect
{
    public class GroupCommandExecutedEventArgs : EventArgs
    {
        public uint GroupId { get; }
        public string MenuItemId { get; }

        public GroupCommandExecutedEventArgs(uint groupId, string menuItemId)
        {
            GroupId = groupId;
            MenuItemId = menuItemId;
        }
    }
}