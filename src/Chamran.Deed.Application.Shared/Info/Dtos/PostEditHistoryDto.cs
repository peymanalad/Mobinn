using Abp.Application.Services.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class PostEditHistoryDto : EntityDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string EditorName { get; set; }
        public DateTime EditTime { get; set; }
        public string Changes { get; set; }

    }
}
