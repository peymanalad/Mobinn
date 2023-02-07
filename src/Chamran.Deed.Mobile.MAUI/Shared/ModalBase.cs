using Chamran.Deed.Core.Dependency;
using Chamran.Deed.Mobile.MAUI.Services.UI;

namespace Chamran.Deed.Mobile.MAUI.Shared
{
    public abstract class ModalBase : DeedComponentBase
    {
        protected ModalManagerService ModalManager { get; set; }

        public abstract string ModalId { get; }

        public ModalBase()
        {
            ModalManager = DependencyResolver.Resolve<ModalManagerService>();
        }

        public virtual async Task Show()
        {
            await ModalManager.Show(JS, ModalId);
            StateHasChanged();
        }

        public virtual async Task Hide()
        {
            await ModalManager.Hide(JS, ModalId);
        }
    }
}
