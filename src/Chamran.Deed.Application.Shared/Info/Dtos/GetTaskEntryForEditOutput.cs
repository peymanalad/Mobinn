using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetTaskEntryForEditOutput
    {
        public CreateOrEditTaskEntryDto TaskEntry { get; set; }

        public string PostPostTitle { get; set; }

        public string UserName { get; set; }

        public string UserName2 { get; set; }

        public string TaskEntryCaption { get; set; }

    }
}