using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetTaskStatForEditOutput
    {
        public CreateOrEditTaskStatDto TaskStat { get; set; }

        public string UserName { get; set; }

    }
}