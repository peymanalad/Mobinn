﻿using Chamran.Deed.Core.Dependency;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Mobile.MAUI.Shared;
using Chamran.Deed.Mobile.MAUI.Models.User;
using Chamran.Deed.Core.Threading;
using Chamran.Deed.Mobile.MAUI.Services.User;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Chamran.Deed.Mobile.MAUI.Pages.User
{
    public partial class Index : DeedMainLayoutPageComponentBase
    {
        protected IUserAppService UserAppService { get; set; }
        protected IUserProfileService UserProfileService { get; set; }
        
        private CreateOrEditUserModal createOrEditUserModal { get; set; }

        private ItemsProviderResult<UserListModel> users;

        private readonly GetUsersInput _filter = new GetUsersInput();

        private Virtualize<UserListModel> UserListContainer { get; set; }

        public Index()
        {
            UserAppService = DependencyResolver.Resolve<IUserAppService>();
            UserProfileService = DependencyResolver.Resolve<IUserProfileService>();
        }

        protected override async Task OnInitializedAsync()
        {
            await SetPageHeader(L("Users"), new List<Services.UI.PageHeaderButton>()
            {
                new Services.UI.PageHeaderButton(L("CreateNewUser"), OpenCreateModal)
            });
        }

        private async Task RefreshList()
        {
            await UserListContainer.RefreshDataAsync();
            StateHasChanged();
        }

        private async ValueTask<ItemsProviderResult<UserListModel>> LoadUsers(ItemsProviderRequest request)
        {
            _filter.MaxResultCount = request.Count;
            _filter.SkipCount = request.StartIndex;

            await UserDialogsService.Block();

            await WebRequestExecuter.Execute(
                async () => await UserAppService.GetUsers(_filter),
                async (result) =>
                {
                    var userList = ObjectMapper.Map<List<UserListModel>>(result.Items);
                    foreach (var user in userList)
                    {
                        await SetUserImageSourceAsync(user);
                    }

                    users = new ItemsProviderResult<UserListModel>(userList, result.TotalCount);

                    await UserDialogsService.UnBlock();
                }
             );

            return users;
        }

        private async Task SetUserImageSourceAsync(UserListModel userListModel)
        {
            if (userListModel.Photo != null)
            {
                return;
            }

            if (!userListModel.ProfilePictureId.HasValue)
            {
                userListModel.Photo = UserProfileService.GetDefaultProfilePicture();
                return;
            }

            userListModel.Photo = await UserProfileService.GetProfilePicture(userListModel.Id);
        }

        public async Task EditUser(UserListModel user)
        {
            await createOrEditUserModal.OpenFor(user);
        }

        public async Task OpenCreateModal()
        {
            await createOrEditUserModal.OpenFor(null);
        }
    }
}
