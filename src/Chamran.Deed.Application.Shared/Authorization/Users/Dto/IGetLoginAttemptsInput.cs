using Abp.Application.Services.Dto;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public interface IGetLoginAttemptsInput: ISortedResultRequest
    {
        string Filter { get; set; }
    }
}