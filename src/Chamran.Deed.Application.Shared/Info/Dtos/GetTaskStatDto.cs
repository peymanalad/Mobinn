using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetTaskStatDto
    {
        public string Caption { get; set; }
        public short Status { get; set; }
        public Guid SharedTaskId { get; set; }
        public long? DoneBy { get; set; }
        public Guid? DoneByProfilePicture { get; set; }
        public string DoneByName { get; set; }
        public string DoneByLastName { get; set; }
        public string DoneByPosition { get; set; }
        public DateTime? CreationTime { get; set; }
    }
}