﻿using System.Threading.Tasks;
using Chamran.Deed.Views;
using Xamarin.Forms;

namespace Chamran.Deed.Services.Modal
{
    public interface IModalService
    {
        Task ShowModalAsync(Page page);

        Task ShowModalAsync<TView>(object navigationParameter) where TView : IXamarinView;

        Task<Page> CloseModalAsync();
    }
}
