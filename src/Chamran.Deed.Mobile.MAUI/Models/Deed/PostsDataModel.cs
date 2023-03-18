using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Chamran.Deed.Mobile.MAUI.Models.Deed
{
    public class PostsDataModel
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string MemberFullName { get; set; }
        public string MemberUserName { get; set; }
        public int GroupMemberId { get; set; }
        public Guid? PostFile { get; set; }
        public string PostCaption { get; set; }
        public int CategoryId { get; set; }
    }
}