using System;
using Abp.Configuration.Startup;

namespace Abp.AspNetZeroCore
{
	// Token: 0x02000007 RID: 7
	public static class AspNetZeroConfigurationExtensions
	{
		// Token: 0x0600000C RID: 12 RVA: 0x0000239C File Offset: 0x0000059C
		public static AspNetZeroConfiguration AspNetZero(this IModuleConfigurations modules)
		{
			return modules.AbpConfiguration.Get<AspNetZeroConfiguration>();
		}
	}
}
