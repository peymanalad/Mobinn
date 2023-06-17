using System;
using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
	// Token: 0x02000003 RID: 3
	[Embedded]
	[CompilerGenerated]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002120 File Offset: 0x00000320
		public NullableAttribute(byte byte_0)
		{
			this.NullableFlags = new byte[] { byte_0 };
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002138 File Offset: 0x00000338
		public NullableAttribute(byte[] byte_0)
		{
			this.NullableFlags = byte_0;
		}

		// Token: 0x04000001 RID: 1
		public readonly byte[] NullableFlags;
	}
}
