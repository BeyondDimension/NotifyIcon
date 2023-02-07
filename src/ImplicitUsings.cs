// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
#if !NET35 && !NET40 && !NET45 && !NETSTANDARD1_0
global using System.Net.Http;
#endif
global using System.Threading;
#if !NET35
global using System.Threading.Tasks;
#endif
#if NET5_0_OR_GREATER2
global using System.Runtime.Versioning;
#endif
