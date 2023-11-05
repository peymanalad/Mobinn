using Chamran.Deed.Authorization.Users;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Common.Exporting;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;

namespace Chamran.Deed.Common
{
    [AbpAuthorize(AppPermissions.Pages_UserTokens)]
    public class UserTokensAppService : DeedAppServiceBase, IUserTokensAppService
    {
        private readonly IRepository<UserToken> _userTokenRepository;
        private readonly IUserTokensExcelExporter _userTokensExcelExporter;
        private readonly IRepository<User, long> _lookup_userRepository;

        public UserTokensAppService(IRepository<UserToken> userTokenRepository, IUserTokensExcelExporter userTokensExcelExporter, IRepository<User, long> lookup_userRepository)
        {
            _userTokenRepository = userTokenRepository;
            _userTokensExcelExporter = userTokensExcelExporter;
            _lookup_userRepository = lookup_userRepository;

        }

        public async Task<PagedResultDto<GetUserTokenForViewDto>> GetAll(GetAllUserTokensInput input)
        {

            var filteredUserTokens = _userTokenRepository.GetAll()
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Token.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TokenFilter), e => e.Token.Contains(input.TokenFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var pagedAndFilteredUserTokens = filteredUserTokens
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var userTokens = from o in pagedAndFilteredUserTokens
                             join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             select new
                             {

                                 o.Token,
                                 Id = o.Id,
                                 UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString()
                             };

            var totalCount = await filteredUserTokens.CountAsync();

            var dbList = await userTokens.ToListAsync();
            var results = new List<GetUserTokenForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetUserTokenForViewDto()
                {
                    UserToken = new UserTokenDto
                    {

                        Token = o.Token,
                        Id = o.Id,
                    },
                    UserName = o.UserName
                };

                results.Add(res);
            }

            return new PagedResultDto<GetUserTokenForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetUserTokenForViewDto> GetUserTokenForView(int id)
        {
            var userToken = await _userTokenRepository.GetAsync(id);

            var output = new GetUserTokenForViewDto { UserToken = ObjectMapper.Map<UserTokenDto>(userToken) };

            if (output.UserToken.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserToken.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens_Edit)]
        public async Task<GetUserTokenForEditOutput> GetUserTokenForEdit(EntityDto input)
        {
            var userToken = await _userTokenRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetUserTokenForEditOutput { UserToken = ObjectMapper.Map<CreateOrEditUserTokenDto>(userToken) };

            if (output.UserToken.UserId != null)
            {
                var _lookupUser = await _lookup_userRepository.FirstOrDefaultAsync((long)output.UserToken.UserId);
                output.UserName = _lookupUser?.Name?.ToString();
            }

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditUserTokenDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens_Create)]
        protected virtual async Task Create(CreateOrEditUserTokenDto input)
        {
            var userToken = ObjectMapper.Map<UserToken>(input);

            await _userTokenRepository.InsertAsync(userToken);

        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens_Edit)]
        protected virtual async Task Update(CreateOrEditUserTokenDto input)
        {
            var userToken = await _userTokenRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, userToken);

        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _userTokenRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetUserTokensToExcel(GetAllUserTokensForExcelInput input)
        {

            var filteredUserTokens = _userTokenRepository.GetAll()
                        .Include(e => e.UserFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Token.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TokenFilter), e => e.Token.Contains(input.TokenFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.UserNameFilter), e => e.UserFk != null && e.UserFk.Name == input.UserNameFilter);

            var query = (from o in filteredUserTokens
                         join o1 in _lookup_userRepository.GetAll() on o.UserId equals o1.Id into j1
                         from s1 in j1.DefaultIfEmpty()

                         select new GetUserTokenForViewDto()
                         {
                             UserToken = new UserTokenDto
                             {
                                 Token = o.Token,
                                 Id = o.Id
                             },
                             UserName = s1 == null || s1.Name == null ? "" : s1.Name.ToString()
                         });

            var userTokenListDtos = await query.ToListAsync();

            return _userTokensExcelExporter.ExportToFile(userTokenListDtos);
        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens)]
        public async Task<PagedResultDto<UserTokenUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_userRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Name != null && e.Name.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var userList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<UserTokenUserLookupTableDto>();
            foreach (var user in userList)
            {
                lookupTableDtoList.Add(new UserTokenUserLookupTableDto
                {
                    Id = user.Id,
                    DisplayName = user.Name?.ToString()
                });
            }

            return new PagedResultDto<UserTokenUserLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_UserTokens_Create)]
        public async Task RegisterDevice(string token)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("Not Logged In!");

            var userToken = new UserToken
            {
                Token = token,
                UserId = AbpSession.UserId
            };
            await _userTokenRepository.InsertAsync(userToken);

        }

    }
}