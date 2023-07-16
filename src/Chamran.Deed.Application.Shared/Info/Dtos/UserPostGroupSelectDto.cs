using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class UserPostGroupSelectDto
    {
        public int GroupId { get; set; }
        public string GroupDescription { get; set; }
        public bool IsSelected { get; set; }
    }
}
