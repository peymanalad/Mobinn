using Chamran.Deed.People;
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

namespace Chamran.Deed.Info
{
    [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
    public class DeedChartsAppService : DeedAppServiceBase, IDeedChartsAppService
    {
        private readonly IRepository<DeedChart> _deedChartRepository;
        private readonly IRepository<Organization, int> _lookup_organizationRepository;
        private readonly IRepository<DeedChart, int> _lookup_deedChartRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;


        public DeedChartsAppService(IRepository<DeedChart> deedChartRepository,
            IRepository<Organization, int> lookup_organizationRepository,
            IRepository<DeedChart, int> lookup_deedChartRepository, IRepository<User, long> userRepository,
            IRepository<GroupMember> groupMemberRepository)
        {
            _deedChartRepository = deedChartRepository;
            _lookup_organizationRepository = lookup_organizationRepository;
            _lookup_deedChartRepository = lookup_deedChartRepository;
            _userRepository = userRepository;
            _groupMemberRepository = groupMemberRepository;
        }

        public virtual async Task<PagedResultDto<GetDeedChartForViewDto>> GetAll(GetAllDeedChartsInput input)
        {
            if (AbpSession.UserId == null) throw new UserFriendlyException("User Must be Logged in!");
            var user = await _userRepository.GetAsync(AbpSession.UserId.Value);

            var filteredDeedCharts = _deedChartRepository.GetAll()
                        .Include(e => e.OrganizationFk)
                        .Include(e => e.ParentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Caption.Contains(input.Filter) || e.LeafPath.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CaptionFilter), e => e.Caption.Contains(input.CaptionFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LeafPathFilter), e => e.LeafPath.Contains(input.LeafPathFilter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OrganizationOrganizationNameFilter), e => e.OrganizationFk != null && e.OrganizationFk.OrganizationName == input.OrganizationOrganizationNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DeedChartCaptionFilter), e => e.ParentFk != null && e.ParentFk.Caption == input.DeedChartCaptionFilter);


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
            //    var headerQuery = from x in _deedChartRepository.GetAll()
            //        where x.OrganizationId == orgEntity.Id
            //        select x;
            //    if (!headerQuery.Any()) throw new UserFriendlyException("شاخه مادر برای سازمان انتخابی یافت نشد");
            //    var headerEntity = headerQuery.First();
            //    filteredDeedCharts =
            //        filteredDeedCharts.Where(x => x.LeafPath.StartsWith(headerEntity.LeafPath));
            //}

            if (!user.IsSuperUser)
            {
                var orgQuery =
                    from org in _lookup_organizationRepository.GetAll().Where(x => !x.IsDeleted)
                    join grpMember in _groupMemberRepository.GetAll() on org.Id equals grpMember.OrganizationId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == AbpSession.UserId
                    select org;
                if (!orgQuery.Any())
                {
                    throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
                }
                // فقط سازمان‌های مربوط به کاربر را فیلتر کنید
                filteredDeedCharts = filteredDeedCharts.Where(x => orgQuery.Any(org => org.Id == x.OrganizationId));
            }

            var pagedAndFilteredDeedCharts = filteredDeedCharts
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var deedCharts = from o in pagedAndFilteredDeedCharts
                             join o1 in _lookup_organizationRepository.GetAll() on o.OrganizationId equals o1.Id into j1
                             from s1 in j1.DefaultIfEmpty()

                             join o2 in _lookup_deedChartRepository.GetAll() on o.ParentId equals o2.Id into j2
                             from s2 in j2.DefaultIfEmpty()

                             select new
                             {
                                 s1.OrganizationLogo,
                                 o.OrganizationId,
                                 o.ParentId,
                                 o.Caption,
                                 o.LeafPath,
                                 Id = o.Id,
                                 OrganizationOrganizationName = s1 == null || s1.OrganizationName == null ? "" : s1.OrganizationName.ToString(),
                                 DeedChartCaption = s2 == null || s2.Caption == null ? "" : s2.Caption.ToString(),
                                 LeafCaptionPath=o.LeafCationPath
                             };

            var totalCount = await filteredDeedCharts.CountAsync();

            var dbList = await deedCharts.ToListAsync();
            var results = new List<GetDeedChartForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetDeedChartForViewDto()
                {
                    DeedChart = new DeedChartDto
                    {
                        OrganizationLogo=o.OrganizationLogo,
                        ParentId = o.ParentId,
                        OrganizationId = o.OrganizationId,
                        Caption = o.Caption,
                        LeafPath = o.LeafPath,
                        LeafCationPath = o.LeafCaptionPath,
                        Id = o.Id,
                    },
                    OrganizationOrganizationName = o.OrganizationOrganizationName,
                    DeedChartCaption = o.DeedChartCaption,

                };

                results.Add(res);
            }

            return new PagedResultDto<GetDeedChartForViewDto>(
                totalCount,
                results
            );

        }

        public virtual async Task<GetDeedChartForViewDto> GetDeedChartForView(int id)
        {
            var deedChart = await _deedChartRepository.GetAsync(id);

            var output = new GetDeedChartForViewDto { DeedChart = ObjectMapper.Map<DeedChartDto>(deedChart) };

            if (output.DeedChart.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.DeedChart.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            if (output.DeedChart.ParentId != null)
            {
                var _lookupDeedChart = await _lookup_deedChartRepository.FirstOrDefaultAsync((int)output.DeedChart.ParentId);
                output.DeedChartCaption = _lookupDeedChart?.Caption?.ToString();
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Edit)]
        public virtual async Task<GetDeedChartForEditOutput> GetDeedChartForEdit(EntityDto input)
        {
            var deedChart = await _deedChartRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetDeedChartForEditOutput { DeedChart = ObjectMapper.Map<CreateOrEditDeedChartDto>(deedChart) };

            if (output.DeedChart.OrganizationId != null)
            {
                var _lookupOrganization = await _lookup_organizationRepository.FirstOrDefaultAsync((int)output.DeedChart.OrganizationId);
                output.OrganizationOrganizationName = _lookupOrganization?.OrganizationName?.ToString();
            }

            if (output.DeedChart.ParentId != null)
            {
                var _lookupDeedChart = await _lookup_deedChartRepository.FirstOrDefaultAsync((int)output.DeedChart.ParentId);
                output.DeedChartCaption = _lookupDeedChart?.Caption?.ToString();
            }

            return output;
        }

        public virtual async Task<int> CreateOrEdit(CreateOrEditDeedChartDto input)
        {
            if (input.Id == null)
            {
                return await Create(input);
            }
            else
            {
                return await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Create)]
        protected virtual async Task<int> Create(CreateOrEditDeedChartDto input)
        {
            var deedChart = ObjectMapper.Map<DeedChart>(input);

            // Fetch the parent organization chart using the ParentId from the input
            if (input.ParentId.HasValue)
            {
                deedChart.ParentFk = await _deedChartRepository.GetAsync(input.ParentId.Value);
            }
            await _deedChartRepository.InsertAsync(deedChart);
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the role.
            deedChart.GenerateLeafPath();
            return deedChart.Id;

        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Edit)]
        protected virtual async Task<int> Update(CreateOrEditDeedChartDto input)
        {
            var deedChart = await _deedChartRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, deedChart);
            return deedChart.Id;
            //deedChart.GenerateLeafPath();

        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _deedChartRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
        public async Task<PagedResultDto<DeedChartOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_organizationRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.OrganizationName != null && e.OrganizationName.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var organizationList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<DeedChartOrganizationLookupTableDto>();
            foreach (var organization in organizationList)
            {
                lookupTableDtoList.Add(new DeedChartOrganizationLookupTableDto
                {
                    Id = organization.Id,
                    DisplayName = organization.OrganizationName?.ToString()
                });
            }

            return new PagedResultDto<DeedChartOrganizationLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
        public async Task<PagedResultDto<DeedChartDeedChartLookupTableDto>> GetAllDeedChartForLookupTable(GetAllForLookupTableInput input)
        {
            var query = _lookup_deedChartRepository.GetAll().WhereIf(
                   !string.IsNullOrWhiteSpace(input.Filter),
                  e => e.Caption != null && e.Caption.Contains(input.Filter)
               );

            var totalCount = await query.CountAsync();

            var deedChartList = await query
                .PageBy(input)
                .ToListAsync();

            var lookupTableDtoList = new List<DeedChartDeedChartLookupTableDto>();
            foreach (var deedChart in deedChartList)
            {
                lookupTableDtoList.Add(new DeedChartDeedChartLookupTableDto
                {
                    Id = deedChart.Id,
                    DisplayName = deedChart.Caption?.ToString()
                });
            }

            return new PagedResultDto<DeedChartDeedChartLookupTableDto>(
                totalCount,
                lookupTableDtoList
            );
        }

        [AbpAuthorize(AppPermissions.Pages_DeedCharts)]
        public async Task SetOrganizationForChartLeaf(SetOrganizationForChartLeafInput input)
        {
            var query = from x in _lookup_deedChartRepository.GetAll()
                where x.Id == input.OrganizationChartId
                select x;
            if (!query.Any()) throw new UserFriendlyException("شاخه مورد نظر یافت نشد");
            query.First().OrganizationId = input.OrganizationId;
            await CurrentUnitOfWork.SaveChangesAsync(); //It's done to get Id of the edition.


        }
    }
}