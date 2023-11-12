using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Chamran.Deed.Info.Dtos;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.People;

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_OrganizationCharts)]
    public class OrganizationChartsAppService : DeedAppServiceBase, IOrganizationChartsAppService
    {
        private readonly IRepository<OrganizationChart> _organizationChartRepository;
        private readonly IRepository<OrganizationChart, int> _lookup_organizationChartRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;
        public OrganizationChartsAppService(IRepository<OrganizationChart> organizationChartRepository, IRepository<OrganizationChart, int> lookup_organizationChartRepository, IRepository<User, long> userRepository, IRepository<Organization, int> lookupOrganizationRepository, IRepository<GroupMember> groupMemberRepository)
        {
            _organizationChartRepository = organizationChartRepository;
            _lookup_organizationChartRepository = lookup_organizationChartRepository;
            _userRepository = userRepository;
            _lookup_organizationRepository = lookupOrganizationRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        public virtual async Task<PagedResultDto<GetOrganizationChartForViewDto>> GetAll(GetAllOrganizationChartsInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

            var filteredOrganizationCharts = _organizationChartRepository.GetAll()
                        .Include(e => e.ParentFk)
                        .Include(e => e.OrganizationFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter) || e.LeafPath.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LeafPathFilter), e => e.LeafPath.Contains(input.LeafPathFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationChartCaptionFilter), e => e.ParentFk != null && e.ParentFk.Caption == input.OrganizationChartCaptionFilter);

            if (!user.IsSuperUser)
            {
                var orgQuery =
                    from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
                    join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
                        .OrganizationId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == AbpSession.UserId
                    select org;

                if (!orgQuery.Any())
                {
                    throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
                }
                var orgEntity = orgQuery.First();
                //filteredOrganizationCharts = filteredOrganizationCharts.Where(x => x.OrganizationId == orgEntity.Id);
                var headerQuery = from x in _organizationChartRepository.GetAll()
                                  where x.OrganizationId == orgEntity.Id
                                  select x;
                if (!headerQuery.Any()) throw new UserFriendlyException("شاخه مادر برای سازمان انتخابی یافت نشد");
                var headerEntity = headerQuery.First();
                filteredOrganizationCharts =
                    filteredOrganizationCharts.Where(x => x.LeafPath.StartsWith(headerEntity.LeafPath));
            }


            var pagedAndFilteredOrganizationCharts = filteredOrganizationCharts
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var organizationCharts = from o in pagedAndFilteredOrganizationCharts
                                     join o1 in _lookup_organizationChartRepository.GetAll() on o.ParentId equals o1.Id into j1
                                     from s1 in j1.DefaultIfEmpty()

                                     select new
                                     {

                                         o.Caption,
                                         o.LeafPath,
                                         o.Id,
                                         OrganizationChartCaption = s1 == null || s1.Caption == null ? "" : s1.Caption.ToString(),
                                         o.OrganizationFk.OrganizationLogo,
                                         OrganizationId = (int?)o.OrganizationFk.Id
                                     };

            var totalCount = await filteredOrganizationCharts.CountAsync();

            var dbList = await organizationCharts.ToListAsync();
            var results = new List<GetOrganizationChartForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationChartForViewDto()
                {
                    OrganizationChart = new OrganizationChartDto
                    {

                        Caption = o.Caption,
                        LeafPath = o.LeafPath,
                        Id = o.Id,
                        OrganizationLogo = o.OrganizationLogo,
                        OrganizationId = o.OrganizationId

                    },
                    OrganizationChartCaption = o.OrganizationChartCaption
                };

                results.Add(res);
            }

            return new PagedResultDto<GetOrganizationChartForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<List<GetOrganizationChartForViewDto>> GetAllForOrganization(int organizationId)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

            var filteredOrganizationCharts = _organizationChartRepository.GetAll()
                        .Include(e => e.ParentFk)
                        .Include(e => e.OrganizationFk)
                        .Where(e => e.OrganizationId == organizationId);

            //if (!user.IsSuperUser)
            //{
            //    var orgQuery =
            //        from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
            //        join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember
            //            .OrganizationId into joined2
            //        from grpMember in joined2.DefaultIfEmpty()
            //        where grpMember.UserId == AbpSession.UserId
            //        select org;

            //    if (!orgQuery.Any())
            //    {
            //        throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
            //    }
            //    var orgEntity = orgQuery.First();
            //    //filteredOrganizationCharts = filteredOrganizationCharts.Where(x => x.OrganizationId == orgEntity.Id);
            //    var headerQuery = from x in _organizationChartRepository.GetAll()
            //                      where x.OrganizationId == orgEntity.Id
            //                      select x;
            //    if (!headerQuery.Any()) throw new UserFriendlyException("شاخه مادر برای سازمان انتخابی یافت نشد");
            //    var headerEntity = headerQuery.First();
            //    filteredOrganizationCharts =
            //        filteredOrganizationCharts.Where(x => x.LeafPath.StartsWith(headerEntity.LeafPath));
            //}


            //var pagedAndFilteredOrganizationCharts = filteredOrganizationCharts
            //    .OrderBy(input.Sorting ?? "id asc")
            //    .PageBy(input);

            var organizationCharts = from o in filteredOrganizationCharts
                                     join o1 in _lookup_organizationChartRepository.GetAll() on o.ParentId equals o1.Id into j1
                                     from s1 in j1.DefaultIfEmpty()

                                     select new
                                     {
                                         o.ParentId,
                                         o.Caption,
                                         o.LeafPath,
                                         o.Id,
                                         OrganizationChartCaption = s1 == null || s1.Caption == null ? "" : s1.Caption.ToString(),
                                         o.OrganizationFk.OrganizationLogo,
                                         OrganizationId = (int?)o.OrganizationFk.Id
                                     };

            //var totalCount = await filteredOrganizationCharts.CountAsync();

            var dbList = await organizationCharts.ToListAsync();
            var results = new List<GetOrganizationChartForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetOrganizationChartForViewDto()
                {
                    OrganizationChart = new OrganizationChartDto
                    {
                        ParentId = o.ParentId,
                        Caption = o.Caption,
                        LeafPath = o.LeafPath,
                        Id = o.Id,
                        OrganizationLogo = o.OrganizationLogo,
                        OrganizationId = o.OrganizationId

                    },
                    OrganizationChartCaption = o.OrganizationChartCaption
                };

                results.Add(res);
            }

            return new List<GetOrganizationChartForViewDto>(results
            );

        }

        public virtual async Task<GetOrganizationChartForViewDto> GetOrganizationChartForView(int id)
        {
            var organizationChart = await _organizationChartRepository.GetAsync(id);

            var output = new GetOrganizationChartForViewDto { OrganizationChart = ObjectMapper.Map<OrganizationChartDto>(organizationChart) };

            if (output.OrganizationChart.ParentId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationChart.ParentId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Edit)]
        public virtual async Task<GetOrganizationChartForEditOutput> GetOrganizationChartForEdit(EntityDto input)
        {
            var organizationChart = await _organizationChartRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetOrganizationChartForEditOutput { OrganizationChart = ObjectMapper.Map<CreateOrEditOrganizationChartDto>(organizationChart) };

            if (output.OrganizationChart.ParentId != null)
            {
                var _lookupOrganizationChart = await _lookup_organizationChartRepository.FirstOrDefaultAsync((int)output.OrganizationChart.ParentId);
                output.OrganizationChartCaption = _lookupOrganizationChart?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditOrganizationChartDto input)
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

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Create)]
        protected virtual async Task Create(CreateOrEditOrganizationChartDto input)
        {
            var organizationChart = ObjectMapper.Map<OrganizationChart>(input);

            // Fetch the parent organization chart using the ParentId from the input
            if (input.ParentId.HasValue)
            {
                organizationChart.ParentFk = await _organizationChartRepository.GetAsync(input.ParentId.Value);
            }
            await _organizationChartRepository.InsertAsync(organizationChart);
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
            organizationChart.GenerateLeafPath();


        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Edit)]
        protected virtual async Task Update(CreateOrEditOrganizationChartDto input)
        {
            var organizationChart = await _organizationChartRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, organizationChart);
            if (string.IsNullOrEmpty(input.LeafPath))
                organizationChart.GenerateLeafPath();


        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _organizationChartRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts)]
        public async Task<PagedResultDto<OrganizationChartOrganizationChartLookupTableDto>> GetAllOrganizationChartForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationChartRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationChartList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<OrganizationChartOrganizationChartLookupTableDto>();
            foreach (var organizationChart in organizationChartList)
            {
                lookupTableDtoList.Add(new OrganizationChartOrganizationChartLookupTableDto
                {
                    Id = organizationChart.Id,
                    DisplayName = organizationChart.Caption?.ToString()
                });
            }

            return new PagedResultDto<OrganizationChartOrganizationChartLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        //[AbpAuthorize(AppPermissions.Pages_OrganizationCharts)]
        //public async Task SetOrganizationForChartLeaf(SetOrganizationForChartLeafInput input)
        //{
        //    var query = from x in _lookup_organizationChartRepository.GetAll()
        //                where x.Id == input.OrganizationChartId
        //                select x;
        //    if (!query.Any()) throw new UserFriendlyException("شاخه مورد نظر یافت نشد");
        //    query.First().OrganizationId = input.OrganizationId;
        //    await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the edition.


        //}

        [AbpAuthorize(AppPermissions.Pages_OrganizationCharts_Create)]
        public virtual async Task<int> CreateCompanyChart(CreateCompanyChartDto createCompanyChartDto)
        {
            //var deedNode = _organizationChartRepository.GetAll().FirstOrDefaultAsync(x => x.ParentId == null);
            //if (deedNode == null) throw new UserFriendlyException("دید تعریف نشده است");

            var entity = await _organizationChartRepository.InsertAsync(new OrganizationChart()
            {
                ParentId = null,
                OrganizationId = createCompanyChartDto.OrganizationId,
                Caption = createCompanyChartDto.Caption,
                
            });
            await CurrentUnitOfWork.SaveChangesAsync(); 
            entity.GenerateLeafPath();
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
            return entity.Id;

        }
    }
}