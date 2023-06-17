using System;
using System.Text.RegularExpressions;
using Abp.Extensions;

namespace Abp.AspNetZeroCore.Validation
{
	// Token: 0x02000008 RID: 8
	public static class ValidationHelper
	{
		// Token: 0x0600000D RID: 13 RVA: 0x000023B8 File Offset: 0x000005B8
		public static bool IsEmail(string value)
		{
			bool flag;
			if (value.IsNullOrEmpty())
			{
				flag = false;
			}
			else
			{
				Regex regex = new Regex("^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$");
				flag = regex.IsMatch(value);
			}
			return flag;
		}

		// Token: 0x04000004 RID: 4
		public const string EmailRegex = "^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$";
	}
}
