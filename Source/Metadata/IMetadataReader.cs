﻿using System;
using System.Reflection;

namespace Bars2Db.Metadata
{
    public interface IMetadataReader
    {
        T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute;
        T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute;
    }
}