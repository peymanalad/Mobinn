using Chamran.Deed.ApiClient;
using Chamran.Deed.Authorization.Accounts;
using Chamran.Deed.Authorization.Accounts.Dto;
using Chamran.Deed.Core.Dependency;
using Chamran.Deed.Core.Threading;
using Chamran.Deed.Mobile.MAUI.Services.UI;
using Chamran.Deed.Mobile.MAUI.Shared;
using Chamran.Deed.Services.Account;
using Chamran.Deed.Services.Navigation;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;

namespace Chamran.Deed.Mobile.MAUI.Pages.Login
{
    public partial class Index : DeedComponentBase
    {
        public bool SmsIsSent { get; set; }
        public string Otp { get; set; }

        //public string UserName
        //{
        //    get => _accountService.AbpAuthenticateModel.UserNameOrEmailAddress;
        //    set
        //    {
        //        _accountService.AbpAuthenticateModel.UserNameOrEmailAddress = value;
        //    }
        //}

        //public string Password
        //{
        //    get => _accountService.AbpAuthenticateModel.Password;
        //    set
        //    {
        //        _accountService.AbpAuthenticateModel.Password = value;
        //    }
        //}

        public string PhoneNumber
        {
            get => _accountService.AbpAuthenticateModel.PhoneNumber;
            set
            {
                _accountService.AbpAuthenticateModel.PhoneNumber = value;
            }
        }

        
        public string Otp1
        {
            get => _accountService.AbpAuthenticateModel.Otp1;
            set
            {
                _accountService.AbpAuthenticateModel.Otp1 = value;
            }
        }
         public string Otp2
        {
            get => _accountService.AbpAuthenticateModel.Otp2;
            set
            {
                _accountService.AbpAuthenticateModel.Otp2 = value;
            }
        }
         public string Otp3
        {
            get => _accountService.AbpAuthenticateModel.Otp3;
            set
            {
                _accountService.AbpAuthenticateModel.Otp3 = value;
            }
        }
         public string Otp4
        {
            get => _accountService.AbpAuthenticateModel.Otp4;
            set
            {
                _accountService.AbpAuthenticateModel.Otp4 = value;
            }
        }
         public string Otp5
        {
            get => _accountService.AbpAuthenticateModel.Otp5;
            set
            {
                _accountService.AbpAuthenticateModel.Otp5 = value;
            }
        }
         public string Otp6
        {
            get => _accountService.AbpAuthenticateModel.Otp6;
            set
            {
                _accountService.AbpAuthenticateModel.Otp6 = value;
            }
        }

        private IAccountService _accountService;
        private IAccountAppService _accountAppService;
        private IApplicationContext _applicationContext;
        private INavigationService _navigationService;

        SwitchTenantModal switchTenantModal;
        EmailActivationModal emailActivationModal;
        ForgotPasswordModal forgotPasswordModal;

        public string CurrentTenancyNameOrDefault => _applicationContext.CurrentTenant != null
        ? _applicationContext.CurrentTenant.TenancyName
        : L("NotSelected");


        public Index()
        {
            _accountService = DependencyResolver.Resolve<IAccountService>();
            _accountAppService = DependencyResolver.Resolve<IAccountAppService>();
            _applicationContext = DependencyResolver.Resolve<IApplicationContext>();
            _navigationService = DependencyResolver.Resolve<INavigationService>();
        }

        protected override async Task OnInitializedAsync()
        {
            await _accountService.LogoutAsync();
            _accountService.AbpAuthenticateModel.TwoFactorVerificationCode = "";
        }

        private async Task LoginUser(string btnId, string phoneInputId)
        {
            //await _accountService.LoginUserAsync();
            if (!SmsIsSent)
            {
                await JS.InvokeVoidAsync("SetButtonTitle", btnId, "تأیید");
                await JS.InvokeVoidAsync("MakeInputReadOnly", phoneInputId);
                SmsIsSent = true;
                StateHasChanged();
                await ShowOnClick();
            }
            else
            {
                _navigationService.NavigateTo(NavigationUrlConsts.Explore);
            }

        }

        private async Task SwitchTenantButton()
        {
            await switchTenantModal.Show();
        }

        private async Task EmailActivationButton()
        {
            await emailActivationModal.Show();
        }

        private async Task ForgotPasswordButton()
        {
            await forgotPasswordModal.Show();
        }

        public async Task OnSwitchTenantSave(string tenantName)
        {
            if (string.IsNullOrEmpty(tenantName))
            {
                _applicationContext.SetAsHost();
                ApiUrlConfig.ResetBaseUrl();
            }
            else
            {
                await SetTenantAsync(tenantName);
            }
        }

        private async Task SetTenantAsync(string tenancyName)
        {
            await SetBusyAsync(async () =>
            {
                await WebRequestExecuter.Execute(
                    async () => await _accountAppService.IsTenantAvailable(
                        new IsTenantAvailableInput { TenancyName = tenancyName }),
                    result => IsTenantAvailableExecuted(result, tenancyName)
                );
            });
        }

        private async Task IsTenantAvailableExecuted(IsTenantAvailableOutput result, string tenancyName)
        {
            var tenantAvailableResult = result;

            switch (tenantAvailableResult.State)
            {
                case TenantAvailabilityState.Available:
                    _applicationContext.SetAsTenant(tenancyName, tenantAvailableResult.TenantId.Value);
                    ApiUrlConfig.ChangeBaseUrl(tenantAvailableResult.ServerRootAddress);
                    break;
                case TenantAvailabilityState.InActive:
                    await UserDialogsService.UnBlock();
                    await UserDialogsService.AlertError(L("TenantIsNotActive", tenancyName));
                    break;
                case TenantAvailabilityState.NotFound:
                    await UserDialogsService.UnBlock();
                    await UserDialogsService.AlertError(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public async Task OnEmailActivation()
        {
            await emailActivationModal.Hide();
            await UserDialogsService.AlertSuccess(L("SendEmailActivationLink_Information"));
        }

        public async Task OnForgotPassword()
        {
            await forgotPasswordModal.Hide();
            await UserDialogsService.AlertSuccess(L("PasswordResetMailSentMessage"));
        }

        private void OnLanguageSwitchAsync()
        {
            StateHasChanged();
        }

        private ToastEffect ShowAnimation = ToastEffect.FadeIn;
        private ToastEffect HideAnimation = ToastEffect.FadeOut;
        SfToast ToastObj;
        private string ToastContent = "رمز عبور به صورت پیامک برای شما ارسال گردید";
        private async Task ShowOnClick()
        {
            await this.ToastObj.ShowAsync();
        }
        private async Task HideOnClick()
        {
            await this.ToastObj.HideAsync("All");
        }
        //private void ShowAnimationChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string> args)
        //{
        //    this.ShowAnimation = (ToastEffect)System.Enum.Parse(typeof(ToastEffect), args.Value);
        //    StateHasChanged();
        //}

        //private void HideAnimationChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string> args)
        //{
        //    this.HideAnimation = (ToastEffect)System.Enum.Parse(typeof(ToastEffect), args.Value);
        //    StateHasChanged();
        //}
    }
}
