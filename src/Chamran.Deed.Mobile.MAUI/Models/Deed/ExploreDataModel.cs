using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Chamran.Deed.Mobile.MAUI.Models.Deed
{
    public class ExploreDataModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string PostGroupDescription { get; set; }
        public Guid? ImageFileId { get; set; }
    }
}
