using System;
using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
	// Token: 0x02000004 RID: 4
	[Embedded]
	[CompilerGenerated]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		// Token: 0x06000004 RID: 4 RVA: 0x00002147 File Offset: 0x00000347
		public NullableContextAttribute(byte byte_0)
		{
			this.Flag = byte_0;
		}

		// Token: 0x04000002 RID: 2
		public readonly byte Flag;
	}
}
