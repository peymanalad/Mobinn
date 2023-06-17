using System;
using Abp.AspNetZeroCore.Licensing;
using Abp.Dependency;
using Abp.Modules;

namespace Abp.AspNetZeroCore
{
	// Token: 0x02000005 RID: 5
	public class AbpAspNetZeroCoreModule : AbpModule
	{
		// Token: 0x06000005 RID: 5 RVA: 0x00002156 File Offset: 0x00000356
		public override void PreInitialize()
		{
			base.IocManager.Register<AspNetZeroConfiguration>(DependencyLifeStyle.Singleton);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002164 File Offset: 0x00000364
		public override void Initialize()
		{
			base.IocManager.RegisterAssemblyByConvention(typeof(AbpAspNetZeroCoreModule).Assembly);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000235C File Offset: 0x0000055C
		public override void PostInitialize()
		{
			//using (IDisposableDependencyObjectWrapper<AspNetZeroWebProjectLicenseChecker> disposableDependencyObjectWrapper = base.IocManager.ResolveAsDisposable<AspNetZeroWebProjectLicenseChecker>())
			//{
			//	disposableDependencyObjectWrapper.Object.Check();
			//}
		}
	}
}
